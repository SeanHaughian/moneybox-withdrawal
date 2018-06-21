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
    public class TransferMoneyTests
    {
        Mock<IAccountRepository> accountRepository;
        Mock<INotificationService> notifcationService;
        User user1, user2;
        Account account1, account2;
        Queue<Account> accounts;

        public void Setup()
        {
            user1 = new User();
            user1.Id = Guid.NewGuid();
            user1.Name = "Sean";
            user1.Email = "Sean@domain.com";
            
            account1 = new Account();
            account1.User = user1;
            account1.Id = Guid.NewGuid();
            account1.Balance = 6000;

            user2 = new User();
            user2.Id = Guid.NewGuid();
            user2.Name = "John";
            user2.Email = "John@domain.com";

            account2 = new Account();
            account2.User = user2;
            account2.Id = Guid.NewGuid();
            account2.Balance = 4000;

            accounts = new Queue<Account>(new Account[] { account1, account2});

            accountRepository = new Mock<IAccountRepository>();
            notifcationService = new Mock<INotificationService>();            
        }

        [Fact]
        public void TransferMoney()
        {
            Setup();
            TransferMoney transferMoney = new TransferMoney(accountRepository.Object, notifcationService.Object);
            accountRepository.Setup(m => m.GetAccountById(account1.Id)).Returns((account1));// => accounts.Dequeue());
            accountRepository.Setup(m => m.GetAccountById(account2.Id)).Returns((account2));
            transferMoney.Execute(account1.Id, account2.Id, 500);
            Assert.Equal(5500, account1.Balance);
            Assert.Equal(4500, account2.Balance);
        }

        [Fact]
        public void TransferMoney_InsufficientFunds()
        {
            Setup();
            TransferMoney transferMoney = new TransferMoney(accountRepository.Object, notifcationService.Object);
            accountRepository.Setup(m => m.GetAccountById(account1.Id)).Returns((account1));// => accounts.Dequeue());
            accountRepository.Setup(m => m.GetAccountById(account2.Id)).Returns((account2));
            Exception ex = Assert.Throws<InvalidOperationException>(() => transferMoney.Execute(account1.Id, account2.Id, 7000));
            Assert.Equal("Insufficient funds to make transfer", ex.Message);
        }

        [Fact]
        public void TransferMoney_PayInLimit()
        {
            Setup();
            TransferMoney transferMoney = new TransferMoney(accountRepository.Object, notifcationService.Object);
            accountRepository.Setup(m => m.GetAccountById(account1.Id)).Returns((account1));// => accounts.Dequeue());
            accountRepository.Setup(m => m.GetAccountById(account2.Id)).Returns((account2));
            Exception ex = Assert.Throws<InvalidOperationException>(() => transferMoney.Execute(account1.Id, account2.Id, 5000));
            Assert.Equal("Account pay in limit reached", ex.Message);
        }

        [Fact]
        public void TransferMoney_FundsLow()
        {
            Setup();
            account1.Balance = 900;
            TransferMoney transferMoney = new TransferMoney(accountRepository.Object, notifcationService.Object);
            accountRepository.Setup(m => m.GetAccountById(account1.Id)).Returns((account1));// => accounts.Dequeue());
            accountRepository.Setup(m => m.GetAccountById(account2.Id)).Returns((account2));
            transferMoney.Execute(account1.Id, account2.Id, 500);
            notifcationService.Verify(m => m.NotifyFundsLow(account1.User.Email));
        }

        [Fact]
        public void TransferMoney_NearPayInLimit()
        {
            Setup();
            TransferMoney transferMoney = new TransferMoney(accountRepository.Object, notifcationService.Object);
            accountRepository.Setup(m => m.GetAccountById(account1.Id)).Returns((account1));// => accounts.Dequeue());
            accountRepository.Setup(m => m.GetAccountById(account2.Id)).Returns((account2));
            transferMoney.Execute(account1.Id, account2.Id, 3600);
            notifcationService.Verify(m => m.NotifyApproachingPayInLimit(account2.User.Email));
        }
    }
}
