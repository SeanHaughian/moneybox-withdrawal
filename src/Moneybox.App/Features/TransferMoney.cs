using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;
        private Object thisLock = new Object();

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            lock (thisLock)
            {
                Account from = this.accountRepository.GetAccountById(fromAccountId);
                Account to = this.accountRepository.GetAccountById(toAccountId);

                decimal fromBalance = from.Balance - amount;
                //If not enough funds to transfer, or pay in limit - throw exception 
                if (fromBalance < 0m)
                {
                    throw new InvalidOperationException("Insufficient funds to make transfer");
                }
                if (amount > Account.PayInLimit)
                {
                    throw new InvalidOperationException("Account pay in limit reached");
                }
                //Otherwise - proceed as normal and throw warning if necessary
                else
                {
                    if (fromBalance < 500m)
                    {
                        this.notificationService.NotifyFundsLow(from.User.Email);
                    }

                    if (Account.PayInLimit - amount < 500m)
                    {
                        this.notificationService.NotifyApproachingPayInLimit(to.User.Email);
                    }

                    from.Balance = from.Balance - amount;
                    to.Balance = to.Balance + amount;

                    this.accountRepository.Update(from);
                    this.accountRepository.Update(to);
                }
            }                   
        }
    }
}
