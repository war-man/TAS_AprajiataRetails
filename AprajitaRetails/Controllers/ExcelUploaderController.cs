﻿using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AprajitaRetails.Models.Data.Voyagers;
using AprajitaRetailsOps.TAS;
using System.Data.Entity;

namespace AprajitaRetailsControllers
{
    public class ExcelUploaderController : Controller
    {
        private VoyagerContext db = new VoyagerContext();


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessPurchase(string dDate)
        {
            DateTime ddDate = DateTime.Parse(dDate).Date;

            InventoryManger iManage = new InventoryManger();
            int a = iManage.ProcessPurchaseInward(ddDate, false);
            //TODO: instead of product item . it should list purchase invoice with item

            if (a > 0)
            {
                var dm = db.ProductItems.Include(c => c.MainCategory);
                ViewBag.MessageHead = "No of Product Item added and stock is created are " + a;
                return View(dm);
            }
            else
            {
                ViewBag.MessageHead = "No Product items added. Some error might has been occured. a=" + a;
                return View(new ProductItem());
            }
        }
        public ActionResult PurchaseList(int? id)
        {
            if (id == 101)
            {
                var md1 = db.ImportPurchases.Where(c => c.IsDataConsumed == true).OrderByDescending(c => c.GRNDate);
                return View(md1);
            }
            else if (id == 100)
            {
                var md1 = db.ImportPurchases.OrderByDescending(c => c.GRNDate).ThenBy(c => c.IsDataConsumed);
                return View(md1);
            }
            var md = db.ImportPurchases.Where(c => c.IsDataConsumed == false).OrderByDescending(c => c.GRNDate);
            return View(md);
        }

        public ActionResult SaleList(int? id)
        {
            var md = db.ImportSaleItemWises.Where(c => c.IsDataConsumed == false).OrderByDescending(c => c.InvoiceDate);
            return View(md);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessSale(string dDate)
        {
            DateTime ddDate = DateTime.Parse(dDate).Date;

            InventoryManger iManage = new InventoryManger();
            int a = iManage.CreateSaleEntry(ddDate);
            if (a > 0)
            {
                var dm = db.SaleInvoices.Include(c=>c.PaymentDetail).Where(c=>c.OnDate==ddDate);
                ViewBag.MessageHead = "No. Of Sale Invoice Created  and item processed are " + a;
                return View(dm.ToList());
            }
            else
            {
                ViewBag.MessageHead = "No Sale items added. Some error might has been occured. a=" + a;
                return View(new SaleInvoice());
            }
            
        }

        // GET: ExcelUploader
        public ActionResult Index()
        {
            //ViewBag.UploadType = new SelectList (UploadType.Types);
            //var vm = db.ImportInWards.OrderByDescending (c => c.ImportInWardId);
            return View();
        }

        [HttpPost]
        public JsonResult UploadExcel(/*string UploadType,*/ UploadTypes UploadType, HttpPostedFileBase FileUpload)
        {
            //UploadType = "InWard";
            List<string> data = new List<string>();
            if (FileUpload != null)
            {
                // tdata.ExecuteCommand("truncate table OtherCompanyAssets");  
                if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    string filename = FileUpload.FileName;
                    string targetpath = Server.MapPath("~/Doc/");
                    FileUpload.SaveAs(targetpath + filename);
                    string pathToExcelFile = targetpath + filename;
                    var connectionString = "";
                    if (filename.EndsWith(".xls"))
                    {
                        connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                    }
                    else if (filename.EndsWith(".xlsx"))
                    {
                        connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                    }

                    var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
                    var ds = new DataSet();

                    adapter.Fill(ds, "ExcelTable");

                    DataTable dtable = ds.Tables["ExcelTable"];

                    string sheetName = "Sheet1";

                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    //if (OfTypes != null && OfTypes == UploadTypes.SaleItemWise)
                    //{
                    //    //alert message for invalid file format  
                    //    data.Add("<ul>");
                    //    data.Add("<li>SaleItemWise of UploadType Was Selected</li>");
                    //    data.Add("</ul>");
                    //    data.ToArray();
                    //    return Json(data, JsonRequestBehavior.AllowGet);
                    //}
                    //else

                    if (UploadType == UploadTypes.Purchase)
                    {
                        var currentImports = from a in excelFile.Worksheet<ImportPurchase>(sheetName) select a;
                        foreach (var a in currentImports)
                        {
                            try
                            {
                                a.ImportTime = DateTime.Now;
                                db.ImportPurchases.Add(a);
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException)
                            {
                                //TODO: need to handel this
                                throw;
                            }
                        }
                    }
                    else if (UploadType == UploadTypes.SaleItemWise)
                    {
                        var currentImports = from a in excelFile.Worksheet<ImportSaleItemWiseVM>(sheetName) select a;
                        foreach (var a in currentImports)
                        {
                            try
                            {
                                a.ImportTime = DateTime.Now;
                                a.IsDataConsumed = false;
                                db.ImportSaleItemWises.Add(ImportSaleItemWiseVM.ToTable(a));
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException)
                            {
                                //TODO: need to handel this
                                throw;
                            }
                        }
                    }
                    else if (UploadType == UploadTypes.SaleRegister)
                    {
                        var currentImports = from a in excelFile.Worksheet<ImportSaleRegister>(sheetName) select a;
                        foreach (var a in currentImports)
                        {
                            try
                            {
                                a.ImportTime = DateTime.Now;
                                a.IsConsumed = false;
                                db.ImportSaleRegisters.Add(a);
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException)
                            {
                                //TODO: need to handel this
                                throw;
                            }
                        }
                    }
                    else if (UploadType == UploadTypes.InWard)
                    {
                        var currentImports = from a in excelFile.Worksheet<ImportInWard>(sheetName) select a;
                        foreach (var a in currentImports)
                        {
                            try
                            {
                                // Inward No   Inward Date Invoice No  Invoice Date    Party Name  Total Qty   Total MRP Value Total Cost
                                ImportInWard inw = new ImportInWard
                                {
                                    ImportDate = DateTime.Today,
                                    InvoiceDate = a.InvoiceDate,
                                    InvoiceNo = a.InvoiceNo,
                                    InWardDate = a.InWardDate,
                                    InWardNo = a.InWardNo,
                                    PartyName = a.PartyName,
                                    TotalCost = a.TotalCost,
                                    TotalMRPValue = a.TotalMRPValue,
                                    TotalQty = a.TotalQty
                                };
                                db.ImportInWards.Add(inw);
                                db.SaveChanges();


                            }
                            catch (DbEntityValidationException)
                            {
                                //TODO: need to handel this

                                throw;
                            }
                        }
                    }
                    else
                    {
                        //alert message for invalid file format  
                        data.Add("<ul>");
                        data.Add("<li>Upload Type select is not supported</li>");
                        data.Add("</ul>");
                        data.ToArray();
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }

                    //deleting excel file from folder  


                    if ((System.IO.File.Exists(pathToExcelFile)))
                    {
                        System.IO.File.Delete(pathToExcelFile);
                    }
                    return Json("success", JsonRequestBehavior.AllowGet);



                }//end of if contexttype
                else
                {
                    //alert message for invalid file format  
                    data.Add("<ul>");
                    data.Add("<li>Only Excel file format is allowed</li>");
                    data.Add("</ul>");
                    data.ToArray();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

            }//end of if fileupload
            else
            {
                data.Add("<ul>");
                if (FileUpload == null)
                    data.Add("<li>Please choose Excel file</li>");
                data.Add("</ul>");
                data.ToArray();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }//end of function
    }//end of class
}//end of namespace