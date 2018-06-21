using Moneybox.App;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace MoneyBox.Test
{
    public class WithdrawMoneyTests
    {
        Mock<IAccountRepository> accountRepository;
        Mock<INotificationService> notifcationService;
        User user, user2;
        Account account, account2;
        Queue<Account> accounts;

        public void Setup()
        {
            user = new User();
            user.Id = Guid.NewGuid();
            user.Name = "Sean";
            user.Email = "Sean@domain.com";

            account = new Account();
            account.User = user;
            account.Id = Guid.NewGuid();
            account.Balance = 6000;

            accountRepository = new Mock<IAccountRepository>();
            notifcationService = new Mock<INotificationService>();
        }

        [Fact]
        public void WithdrawMoney()
        {
            Setup();
            WithdrawMoney withdrawMoney = new WithdrawMoney(accountRepository.Object, notifcationService.Object);
            accountRepository.Setup(m => m.GetAccountById(account.Id)).Returns((account));// => accounts.Dequeue());
            withdrawMoney.Execute(account.Id, 500);
            Assert.Equal(5500, account.Balance);
        }

        [Fact]
        public void WithdrawMoney_InsufficientFunds()
        {
            Setup();
            account.Balance = 900;
            WithdrawMoney withdrawMoney = new WithdrawMoney(accountRepository.Object, notifcationService.Object);
            accountRepository.Setup(m => m.GetAccountById(account.Id)).Returns((account));// => accounts.Dequeue());
            Exception ex = Assert.Throws<InvalidOperationException>(() => withdrawMoney.Execute(account.Id, 7000));
            Assert.Equal("Insufficient funds to make withdrawal", ex.Message);
        }

        [Fact]
        public void WithdrawMoney_FundsLow()
        {
            Setup();
            account.Balance = 900;
            WithdrawMoney withdrawMoney = new WithdrawMoney(accountRepository.Object, notifcationService.Object);
            accountRepository.Setup(m => m.GetAccountById(account.Id)).Returns((account));// => accounts.Dequeue());
            withdrawMoney.Execute(account.Id, 500);
            notifcationService.Verify(m => m.NotifyFundsLow(account.User.Email));
        }
    }
}
