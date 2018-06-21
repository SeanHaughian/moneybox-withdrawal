using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney
    {
        private IAccountRepository accountRepository;
        private INotificationService notificationService;
        private Object thisLock = new Object();

        public WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            this.accountRepository = accountRepository;
            this.notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            lock (thisLock)
            {
                Account account = this.accountRepository.GetAccountById(fromAccountId);
                decimal balance = account.Balance - amount;

                //If not enough funds to withdrawal - throw exception 
                if (balance < 0m)
                {
                    throw new InvalidOperationException("Insufficient funds to make withdrawal");
                }

                else
                {
                    if (balance < 500m)
                    {
                        this.notificationService.NotifyFundsLow(account.User.Email);
                    }
                    account.Balance = account.Balance - amount;

                    this.accountRepository.Update(account);
                }
            }
        }
    }
}
