using CashDesk.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashDesk
{
    /// <inheritdoc />
    public class DataAccess : IDataAccess
    {
        private CashDeskDataContext _context;

        /// <inheritdoc />
        public async Task InitializeDatabaseAsync() {
            if (_context != null)
            {
                throw new InvalidOperationException();
            }
            _context = new CashDeskDataContext();
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<int> AddMemberAsync(string firstName, string lastName, DateTime birthday)
        {
            if(_context == null)
            {
                throw new InvalidOperationException();
            }
            if (_context.Members.Count(member => member.LastName == lastName) > 0)
            {
                throw new DuplicateNameException();
            }
            if(firstName == null || lastName == null || birthday == null)
            {
                throw new ArgumentException("Value NULL is not allowed");
            }

            var result = await _context.AddAsync(new Member
            {
               FirstName = firstName,
               LastName = lastName,
               Birthday = birthday

            });
            await _context.SaveChangesAsync();
            return result.Entity.MemberNumber;
        }

        /// <inheritdoc />
        public async Task DeleteMemberAsync(int memberNumber)
        {
            if (_context == null)
            {
                throw new InvalidOperationException();
            }
            if (await _context.Members.FindAsync(memberNumber) == null)
            {
                throw new ArgumentException("Unknown memberNumber");
            }
            var member = await _context.Members.FindAsync(memberNumber);
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IMembership> JoinMemberAsync(int memberNumber)
        {
            if (_context == null)
            {
                throw new InvalidOperationException();
            }
            if (await _context.Members.FindAsync(memberNumber) == null)
            {
                throw new ArgumentException("Unknown memberNumber");
            }
            var membership = _context.Memberships.FirstOrDefault(membership => membership.MemberId == memberNumber);
            if (membership != null)
            {
                if (membership.End != default(DateTime) && membership.End != null)
                {
                    membership.Begin = System.DateTime.Now;
                    membership.End = default;
                    var result = _context.Update(membership);
                    await _context.SaveChangesAsync();
                    return result.Entity;
                }
                else
                {
                    throw new AlreadyMemberException();
                }
            }
            else
            {
                var result = await _context.AddAsync(new Membership
                {
                    Member = await _context.Members.FindAsync(memberNumber),
                    Begin = System.DateTime.Now,
                    MemberId = memberNumber
                });
                await _context.SaveChangesAsync();
                return result.Entity;
            }
        }

        /// <inheritdoc />
        public async Task<IMembership> CancelMembershipAsync(int memberNumber)
        {
            if (_context == null)
            {
                throw new InvalidOperationException();
            }
            if (await _context.Members.FindAsync(memberNumber) == null)
            {
                throw new ArgumentException("Unknown memberNumber");
            }
            var membership = _context.Memberships.FirstOrDefault(membership => membership.MemberId == memberNumber);
            if (membership == null)
            {
                throw new NoMemberException();
            }
            membership.End = System.DateTime.Now;
            _context.Update(membership);
            await _context.SaveChangesAsync();
            return membership;
        }

        /// <inheritdoc />
        public async Task DepositAsync(int memberNumber, decimal amount)
        {
            if (_context == null)
            {
                throw new InvalidOperationException();
            }
            var member = await _context.Members.FindAsync(memberNumber);
            if (member == null || amount <= 0)
            {
                throw new ArgumentException("Invalid Arguments");
            }
            var membership = _context.Memberships.FirstOrDefault(membership => membership.MemberId == memberNumber);
            if (membership == null)
            {
                throw new NoMemberException();
            }
            await _context.AddAsync(new Deposit
            {
                Membership = membership,
                Amount = amount
            });
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDepositStatistics>> GetDepositStatisticsAsync()
        {
            if (_context == null)
            {
                throw new InvalidOperationException();
            }
            Dictionary<int, DepositStatistics> statistics = new Dictionary<int, DepositStatistics>();

            foreach (var deposit in _context.Deposits)
            {
                if (deposit.Membership == null) continue;
                if (statistics.ContainsKey(deposit.Membership.MemberId))
                {
                    statistics[deposit.Membership.MemberId].TotalAmount += deposit.Amount;
                }
                else
                {
                    statistics.Add(deposit.Membership.MemberId, new DepositStatistics
                    {
                        Member = deposit.Membership.Member,
                        Year = deposit.Membership.Begin.Year,
                        TotalAmount = deposit.Amount
                    });
                }
            }
            return statistics.Values.ToArray();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_context != null)
            {
                _context.Dispose();
            }
        }
    }
}
