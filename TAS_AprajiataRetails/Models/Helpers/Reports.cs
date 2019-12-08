﻿using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using TAS_AprajiataRetails.Models.Data;
using TAS_AprajiataRetails.Models.Views;

namespace TAS_AprajiataRetails.Models.Helpers
{
    public class Reports
    {
        public static TailoringReport GetTailoringReport()
        {
            using (AprajitaRetailsContext db = new AprajitaRetailsContext())
            {
                return new TailoringReport()
                {
                    TodayBooking = (int?)db.Bookings.Where(c => DbFunctions.TruncateTime(c.BookingDate) == DbFunctions.TruncateTime(DateTime.Today)).Count() ?? 0,
                    TodayUnit = (int?)db.Bookings.Where(c => DbFunctions.TruncateTime(c.BookingDate) == DbFunctions.TruncateTime(DateTime.Today)).Sum(c => (int?)c.TotalQty) ?? 0,

                    MonthlyBooking = (int?)db.Bookings.Where(c => DbFunctions.TruncateTime(c.BookingDate).Value.Month == DbFunctions.TruncateTime(DateTime.Today).Value.Month).Count() ?? 0,
                    MonthlyUnit = (int?)db.Bookings.Where(c => DbFunctions.TruncateTime(c.BookingDate).Value.Month == DbFunctions.TruncateTime(DateTime.Today).Value.Month).Sum(c => (int?)c.TotalQty) ?? 0,
                    YearlyBooking = (int?)db.Bookings.Where(c => DbFunctions.TruncateTime(c.BookingDate).Value.Month == DbFunctions.TruncateTime(DateTime.Today).Value.Month).Count() ?? 0,
                    YearlyUnit = (int?)db.Bookings.Where(c => DbFunctions.TruncateTime(c.BookingDate).Value.Month == DbFunctions.TruncateTime(DateTime.Today).Value.Month).Sum(c => (int?)c.TotalQty) ?? 0,

                    TodaySale = (decimal?)db.Deliveries.Where(c => DbFunctions.TruncateTime(c.DeliveryDate) == DbFunctions.TruncateTime(DateTime.Today)).Sum(c => (decimal?)c.Amount) ?? 0,
                    YearlySale = (decimal?)db.Deliveries.Where(c => DbFunctions.TruncateTime(c.DeliveryDate).Value.Year == DbFunctions.TruncateTime(DateTime.Today).Value.Year).Sum(c => (decimal?)c.Amount) ?? 0,
                    MonthlySale = (decimal?)db.Deliveries.Where(c => DbFunctions.TruncateTime(c.DeliveryDate).Value.Month == DbFunctions.TruncateTime(DateTime.Today).Value.Month).Sum(c => (decimal?)c.Amount) ?? 0,
                };

            }


        }

        public static DailySaleReport GetSaleRecord()
        {
            using (AprajitaRetailsContext db = new AprajitaRetailsContext())
            {
                DailySaleReport record = new DailySaleReport();
                record.DailySale = (decimal?)db.DailySales.Where(C => DbFunctions.TruncateTime(C.SaleDate) == DbFunctions.TruncateTime(DateTime.Today)).Sum(c => (decimal?)c.Amount) ?? 0;
                record.MonthlySale = (decimal?)db.DailySales.Where(C => DbFunctions.TruncateTime(C.SaleDate).Value.Month == DbFunctions.TruncateTime(DateTime.Today).Value.Month).Sum(c => (decimal?)c.Amount) ?? 0;
                record.YearlySale = (decimal?)db.DailySales.Where(C => DbFunctions.TruncateTime(C.SaleDate).Value.Year == DbFunctions.TruncateTime(DateTime.Today).Value.Year).Sum(c => (decimal?)c.Amount) ?? 0;

                return record;
            }

        }

        public static void GetAccoutingRecord()
        {

        }


        public static void GetEmpInfo()
        {
            using (AprajitaRetailsContext db = new AprajitaRetailsContext())
            {
                //List<EmployeeInfo> EmpInfoList = new List<EmployeeInfo>();
                //var emps= db.Employees.Where(c=>DbFunctions.TruncateTime( c.AttDate).Value.Month== DbFunctions.TruncateTime(DateTime.Today).Value.Month);
                //foreach (var emp in emps)
                //{
                //    EmployeeInfo info = new EmployeeInfo()
                //    {
                //        Name=emp.Employee.StaffName
                //    };

                //}


            }
        }


        public static List<CashBook> GetMontlyCashBook(DateTime date)
        {
            using (AprajitaRetailsContext db = new AprajitaRetailsContext())
            {
                List<CashBook> book = new List<CashBook>();


                DateTime oDate = new DateTime(date.Year, date.Month, 1);
                decimal OpnBal = 0;
                try
                {
                    OpnBal = (decimal?)db.CashInHands.Where(c => DbFunctions.TruncateTime(c.CIHDate) == DbFunctions.TruncateTime(oDate)).FirstOrDefault().OpenningBalance ?? 0;

                }
                catch (Exception)
                {
                    OpnBal = 0;
                }
                //income
                var dSale = db.DailySales.Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.SaleDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.SaleDate);//ok
                var dRec = db.Receipts.Where(c => c.PayMode == PaymentModes.Cash && DbFunctions.TruncateTime(c.RecieptDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.RecieptDate);//ok
                var dCashRec = db.CashReceipts.Where(c => DbFunctions.TruncateTime(c.InwardDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.InwardDate);//ok
                var dSRec = db.StaffAdvanceReceipts.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.ReceiptDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.ReceiptDate);//ok
                var dWit = db.Withdrawals.Include(C => C.Account).Where(c => DbFunctions.TruncateTime(c.DepoDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.DepoDate);
                var dTalRec = db.TailoringStaffAdvanceReceipts.Include(c => c.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.ReceiptDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.ReceiptDate);//ok
                foreach (var item in dTalRec)
                {
                    CashBook b = new CashBook() { EDate = item.ReceiptDate, CashIn = item.Amount, Particulars = item.Employee.StaffName, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in dSale)
                {
                    CashBook b = new CashBook() { EDate = item.SaleDate, CashIn = item.Amount, Particulars = item.InvNo, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in dRec)
                {
                    CashBook b = new CashBook() { EDate = item.RecieptDate, CashIn = item.Amount, Particulars = item.ReceiptFrom, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in dCashRec)
                {
                    CashBook b = new CashBook() { EDate = item.InwardDate, CashIn = item.Amount, Particulars = item.ReceiptFrom, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in dSRec)
                {
                    CashBook b = new CashBook() { EDate = item.ReceiptDate, CashIn = item.Amount, Particulars = item.Employee.StaffName, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in dWit)
                {
                    CashBook b = new CashBook() { EDate = item.DepoDate, CashIn = item.Amount, Particulars = "Bank=> " + item.Account.Account, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }


                //Expenses

                var eCPay = db.CashPayments.Where(c => DbFunctions.TruncateTime(c.PaymentDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.PaymentDate);//ok
                var ePay = db.Payments.Where(c => c.PayMode == PaymentModes.Cash && DbFunctions.TruncateTime(c.PayDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.PayDate);
                var eStPay = db.StaffAdvancePayments.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.PaymentDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.PaymentDate);
                var eSal = db.Salaries.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.PaymentDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.PaymentDate);
                var eexp = db.Expenses.Where(c => c.PayMode == PaymentModes.Cash && DbFunctions.TruncateTime(c.ExpDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.ExpDate);
                var eDepo = db.BankDeposits.Include(C => C.Account).Where(c => DbFunctions.TruncateTime(c.DepoDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.DepoDate);
                var eDue = db.DuesLists.Include(e => e.DailySale).Where(c => c.IsRecovered == false && DbFunctions.TruncateTime(c.DailySale.SaleDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.DailySale.SaleDate);
                var eCashEx = db.CashExpenses.Where(c => DbFunctions.TruncateTime(c.ExpDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.ExpDate);

                var eTalSal = db.TailoringSalaries.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.PaymentDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.PaymentDate);
                var eTalStPay = db.TailoringStaffAdvancePayments.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.PaymentDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.PaymentDate);

                foreach (var item in eTalStPay)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, Particulars = item.Employee.StaffName, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in eTalSal)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, Particulars = item.Employee.StaffName, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }


                foreach (var item in eexp)
                {
                    CashBook b = new CashBook() { EDate = item.ExpDate, CashIn = 0, Particulars = item.Particulars, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in eDepo)
                {
                    CashBook b = new CashBook() { EDate = item.DepoDate, CashIn = 0, Particulars = "Bank Depo=> " + item.Account.Account, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in eCashEx)
                {
                    CashBook b = new CashBook() { EDate = item.ExpDate, CashIn = 0, Particulars = item.Particulars, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in eDue)
                {
                    CashBook b = new CashBook() { EDate = item.DailySale.SaleDate, CashIn = 0, Particulars = "Dues=>" + item.DailySale.InvNo, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }


                foreach (var item in eCPay)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, Particulars = item.PaidTo, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in ePay)
                {
                    CashBook b = new CashBook() { EDate = item.PayDate, CashIn = 0, Particulars = item.PaymentPartry, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in eStPay)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, Particulars = item.Employee.StaffName, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in eSal)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, Particulars = item.Employee.StaffName, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }
                return CorrectBalCashBook(book, OpnBal);

            }//end of using

        }

        public static List<CashBook> GetDailyCashBook(DateTime date)
        {
            using (AprajitaRetailsContext db = new AprajitaRetailsContext())
            {
                List<CashBook> book = new List<CashBook>();
                decimal OpnBal = 0;
                try
                {
                    OpnBal = (decimal?)db.CashInHands.Where(c => DbFunctions.TruncateTime(c.CIHDate) == DbFunctions.TruncateTime(date)).FirstOrDefault().OpenningBalance ?? 0;

                }
                catch (Exception)
                {
                    OpnBal = 0;
                }

                // decimal OpnBal= (decimal?)db.CashInHands.Where(c => DbFunctions.TruncateTime(c.CIHDate) == DbFunctions.TruncateTime(date)).FirstOrDefault().OpenningBalance??0;
                //income
                var dSale = db.DailySales.Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.SaleDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.SaleDate);//ok
                var dRec = db.Receipts.Where(c => c.PayMode == PaymentModes.Cash && DbFunctions.TruncateTime(c.RecieptDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.RecieptDate);//ok
                var dCashRec = db.CashReceipts.Where(c => DbFunctions.TruncateTime(c.InwardDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.InwardDate);//ok
                var dSRec = db.StaffAdvanceReceipts.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.ReceiptDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.ReceiptDate);//ok
                var dWit = db.Withdrawals.Include(C => C.Account).Where(c => DbFunctions.TruncateTime(c.DepoDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.DepoDate);
                var dTalRec = db.TailoringStaffAdvanceReceipts.Include(c => c.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.ReceiptDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.ReceiptDate);//ok

                foreach (var item in dTalRec)
                {
                    CashBook b = new CashBook() { EDate = item.ReceiptDate, CashIn = item.Amount, Particulars = item.Employee.StaffName, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in dSale)
                {
                    CashBook b = new CashBook() { EDate = item.SaleDate, CashIn = item.Amount, Particulars = item.InvNo, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in dRec)
                {
                    CashBook b = new CashBook() { EDate = item.RecieptDate, CashIn = item.Amount, Particulars = item.ReceiptFrom, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in dCashRec)
                {
                    CashBook b = new CashBook() { EDate = item.InwardDate, CashIn = item.Amount, Particulars = item.ReceiptFrom, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in dSRec)
                {
                    CashBook b = new CashBook() { EDate = item.ReceiptDate, CashIn = item.Amount, Particulars = item.Employee.StaffName, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }




                foreach (var item in dWit)
                {
                    CashBook b = new CashBook() { EDate = item.DepoDate, CashIn = item.Amount, Particulars = item.Account.Account, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }




                //Expenses

                var eCPay = db.CashPayments.Where(c => DbFunctions.TruncateTime(c.PaymentDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.PaymentDate);//ok
                var ePay = db.Payments.Where(c => c.PayMode == PaymentModes.Cash && DbFunctions.TruncateTime(c.PayDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.PayDate);
                var eStPay = db.StaffAdvancePayments.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.PaymentDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.PaymentDate);
                var eSal = db.Salaries.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.PaymentDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.PaymentDate);
                var eexp = db.Expenses.Where(c => c.PayMode == PaymentModes.Cash && DbFunctions.TruncateTime(c.ExpDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.ExpDate);
                var eDepo = db.BankDeposits.Include(C => C.Account).Where(c => DbFunctions.TruncateTime(c.DepoDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.DepoDate);
                var eDue = db.DuesLists.Include(c => c.DailySale).Where(c => c.IsRecovered == false && DbFunctions.TruncateTime(c.DailySale.SaleDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.DailySale.SaleDate);
                var eCashEx = db.CashExpenses.Where(c => DbFunctions.TruncateTime(c.ExpDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.ExpDate);

                var eTalSal = db.TailoringSalaries.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.PaymentDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.PaymentDate);
                var eTalStPay = db.TailoringStaffAdvancePayments.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.PaymentDate) == DbFunctions.TruncateTime(date)).OrderBy(c => c.PaymentDate);

                foreach (var item in eTalStPay)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, Particulars = item.Employee.StaffName, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in eTalSal)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, Particulars = item.Employee.StaffName, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }


                foreach (var item in eexp)
                {
                    CashBook b = new CashBook() { EDate = item.ExpDate, CashIn = 0, Particulars = item.PaidTo, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in eDepo)
                {
                    CashBook b = new CashBook() { EDate = item.DepoDate, CashIn = 0, Particulars = "Bank Depo" + item.Account.Account, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in eCashEx)
                {
                    CashBook b = new CashBook() { EDate = item.ExpDate, CashIn = 0, Particulars = item.PaidTo, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in eDue)
                {
                    CashBook b = new CashBook() { EDate = item.DailySale.SaleDate, CashIn = 0, Particulars = "Dues " + item.DailySale.InvNo, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }


                foreach (var item in eCPay)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, Particulars = item.PaidTo, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in ePay)
                {
                    CashBook b = new CashBook() { EDate = item.PayDate, CashIn = 0, Particulars = item.PaymentPartry, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in eStPay)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, Particulars = item.Employee.StaffName, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in eSal)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, Particulars = item.Employee.StaffName, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }


                return CorrectBalCashBook(book, OpnBal);
            } //end of usin

        }

        public static List<CashBook> CorrectBalCashBook(List<CashBook> books, decimal OpnBal)
        {
            IEnumerable<CashBook> orderBook = books.OrderBy(c => c.EDate);

            decimal bal = OpnBal;
            foreach (var item in orderBook)
            {
                item.CashBalance = bal + item.CashIn - item.CashOut;
                bal = item.CashBalance;
            }
            return orderBook.ToList();


        }


        public static List<CashBook> CorrectCashInHand_OnDB(DateTime date)
        {

            using (AprajitaRetailsContext db = new AprajitaRetailsContext())
            {
                List<CashBook> book = new List<CashBook>();

                //income
                var dSale = db.DailySales.Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.SaleDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.SaleDate);//ok
                var dRec = db.Receipts.Where(c => c.PayMode == PaymentModes.Cash && DbFunctions.TruncateTime(c.RecieptDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.RecieptDate);//ok
                var dCashRec = db.CashReceipts.Where(c => DbFunctions.TruncateTime(c.InwardDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.InwardDate);//ok
                var dSRec = db.StaffAdvanceReceipts.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.ReceiptDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.ReceiptDate);//ok
                var dWit = db.Withdrawals.Include(C => C.Account).Where(c => DbFunctions.TruncateTime(c.DepoDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.DepoDate);

                var dTalRec = db.TailoringStaffAdvanceReceipts.Include(c => c.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.ReceiptDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.ReceiptDate);//ok

                foreach (var item in dSale)
                {
                    CashBook b = new CashBook() { EDate = item.SaleDate, CashIn = item.Amount, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in dRec)
                {
                    CashBook b = new CashBook() { EDate = item.RecieptDate, CashIn = item.Amount, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in dCashRec)
                {
                    CashBook b = new CashBook() { EDate = item.InwardDate, CashIn = item.Amount, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in dSRec)
                {
                    CashBook b = new CashBook() { EDate = item.ReceiptDate, CashIn = item.Amount, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in dTalRec)
                {
                    CashBook b = new CashBook() { EDate = item.ReceiptDate, CashIn = item.Amount, CashOut = 0, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in dWit)
                {
                    CashBook b = new CashBook() { EDate = item.DepoDate, CashIn = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }


                //Expenses

                var eCPay = db.CashPayments.Where(c => DbFunctions.TruncateTime(c.PaymentDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.PaymentDate);//ok
                var ePay = db.Payments.Where(c => c.PayMode == PaymentModes.Cash && DbFunctions.TruncateTime(c.PayDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.PayDate);
                var eStPay = db.StaffAdvancePayments.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.PaymentDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.PaymentDate);
                var eSal = db.Salaries.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.PaymentDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.PaymentDate);
                var eexp = db.Expenses.Where(c => c.PayMode == PaymentModes.Cash && DbFunctions.TruncateTime(c.ExpDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.ExpDate);
                var eDepo = db.BankDeposits.Include(C => C.Account).Where(c => DbFunctions.TruncateTime(c.DepoDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.DepoDate);
                var eDue = db.DuesLists.Include(e => e.DailySale).Where(c => c.IsRecovered == false && DbFunctions.TruncateTime(c.DailySale.SaleDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.DailySale.SaleDate);
                var eCashEx = db.CashExpenses.Where(c => DbFunctions.TruncateTime(c.ExpDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.ExpDate);

                var eTalSal = db.TailoringSalaries.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.PaymentDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.PaymentDate);
                var eTalStPay = db.TailoringStaffAdvancePayments.Include(e => e.Employee).Where(c => c.PayMode == PayModes.Cash && DbFunctions.TruncateTime(c.PaymentDate).Value.Month == DbFunctions.TruncateTime(date).Value.Month).OrderBy(c => c.PaymentDate);

                foreach (var item in eTalStPay)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in eTalSal)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }




                foreach (var item in eexp)
                {
                    CashBook b = new CashBook() { EDate = item.ExpDate, CashIn = 0, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in eDepo)
                {
                    CashBook b = new CashBook() { EDate = item.DepoDate, CashIn = 0, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in eCashEx)
                {
                    CashBook b = new CashBook() { EDate = item.ExpDate, CashIn = 0, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }
                foreach (var item in eDue)
                {
                    CashBook b = new CashBook() { EDate = item.DailySale.SaleDate, CashIn = 0, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }


                foreach (var item in eCPay)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in ePay)
                {
                    CashBook b = new CashBook() { EDate = item.PayDate, CashIn = 0, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in eStPay)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }

                foreach (var item in eSal)
                {
                    CashBook b = new CashBook() { EDate = item.PaymentDate, CashIn = 0, CashOut = item.Amount, CashBalance = 0 };
                    book.Add(b);
                }
               return UpdateCorrectCashInHand_DB(book,db);

            }//end of using

        }

        public static List<CashBook> UpdateCorrectCashInHand_DB(List<CashBook> books, AprajitaRetailsContext db)
        {
            
            IEnumerable<CashBook> orderBook = books.OrderBy(c => c.EDate);

            decimal bal = 0;
            DateTime dDate = orderBook.First().EDate.AddDays(-1);

            var cIh = db.CashInHands.Where(c => DbFunctions.TruncateTime(c.CIHDate) == DbFunctions.TruncateTime(dDate)).FirstOrDefault();
            if (cIh != null)
            {
                bal =cIh.ClosingBalance= cIh.OpenningBalance + cIh.CashIn - cIh.CashOut;
            }
            dDate = orderBook.First().EDate;
            cIh=db.CashInHands.Where(c => DbFunctions.TruncateTime(c.CIHDate) == DbFunctions.TruncateTime(dDate)).FirstOrDefault();
            if (cIh == null)
            {
                Utils.CreateCashInHand(db, dDate, 0, 0, true);
            }
            else
            {
                cIh.CashIn = 0;
                cIh.CashOut = 0;
                cIh.OpenningBalance = bal;
                cIh.ClosingBalance = cIh.OpenningBalance + cIh.CashIn - cIh.CashOut;

            }
            db.SaveChanges();
            DateTime tDate = dDate.AddDays(-1);
            var cashInHandList = db.CashInHands.Where(c => DbFunctions.TruncateTime(c.CIHDate) > DbFunctions.TruncateTime(tDate));

            decimal opibal = 0;
            foreach (var item in orderBook)
            {
                if (dDate != item.EDate)
                {
                    dDate = item.EDate;
                    CashInHand cs = cashInHandList.Where(c => DbFunctions.TruncateTime(c.CIHDate) == DbFunctions.TruncateTime(item.EDate)).FirstOrDefault();
                    
                    cs.CashIn =item.CashIn;
                    cs.CashOut = item.CashOut;

                    tDate = item.EDate.AddDays(-1);
                    cs.OpenningBalance = opibal;
                    opibal= cs.ClosingBalance = cs.OpenningBalance + cs.CashIn - cs.CashOut;
                    
                    db.Entry(cs).State = EntityState.Modified;
                }
                else
                {
                    CashInHand cs = cashInHandList.Where(c => DbFunctions.TruncateTime(c.CIHDate) == item.EDate).FirstOrDefault();
                    cs.CashIn = item.CashIn;
                    cs.CashOut = item.CashOut;
                    opibal=cs.ClosingBalance = cs.OpenningBalance + cs.CashIn - cs.CashOut;
                    db.Entry(cs).State = EntityState.Modified;
                }

                item.CashBalance = bal + item.CashIn - item.CashOut;
                bal = item.CashBalance;
            }
            db.SaveChanges();
            return orderBook.ToList();


        }


    }


}