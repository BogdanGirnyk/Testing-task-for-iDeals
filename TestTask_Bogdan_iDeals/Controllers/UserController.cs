using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TestTask_Bogdan_iDeals.ViewModels;

namespace TestTask_Bogdan_iDeals.Controllers
{
    public class UserController : Controller
    {
        [AllowAnonymous]
        public ActionResult Create()
        {
            if (User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
            }

               return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Create(UserCreate model)
        {
            if (ModelState.IsValid)
            {
                DBActionStatus<bool> result = DBAccess.CreateUser(model);

                if (result.status == ResultType.Success)
                {
                    DBAccess.WriteLog("Event", result.message, DateTime.Now);
                    FormsAuthentication.SetAuthCookie(model.Email, true);
                    return RedirectToAction("Get", "User");
                }
                else
                {
                    DBAccess.WriteLog("Error", result.message, DateTime.Now);
                    ModelState.AddModelError("", result.message);
                    model.Password = "";
                    model.Passwordrepeat = "";
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid data");
                model.Password = "";
                model.Passwordrepeat = "";
                return View(model);
            }
        }


        [Authorize]
        public ActionResult Get()
        {
            string currEmail = User.Identity.Name;
            DBActionStatus<UserGetUpdate> result = DBAccess.GetUser(currEmail);
            if (result.status == ResultType.Success)
            {
                return View(result.data);
            }
            else
            {
                DBAccess.WriteLog("Error", result.message, DateTime.Now);
                ModelState.AddModelError("", result.message);
                return View(new UserGetUpdate());
            }

        }

        [AllowAnonymous]
        public ActionResult Login()
        {

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin model)
        {
            if (ModelState.IsValid)
            {
                DBActionStatus<bool> result = DBAccess.CheckLogin(model.Email,model.Password);
                if (result.status == ResultType.Success)
                {
                    DBAccess.WriteLog("Event", result.message, DateTime.Now);
                    FormsAuthentication.SetAuthCookie(model.Email, true);
                    return RedirectToAction("Get","User");
                }
                else
                {
                    DBAccess.WriteLog("Error", result.message, DateTime.Now);
                    ModelState.AddModelError("", "Wrong email/password");
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid data");
            }
            return View(model);
        }

        public ActionResult Logout()
        {
            string currEmail = User.Identity.Name;
            FormsAuthentication.SignOut();
            DBAccess.WriteLog("Event", "Logged off: "+currEmail, DateTime.Now);
            return RedirectToAction("Login", "User");
        }

        [Authorize]
        public ActionResult Update()
        {
            string currEmail = User.Identity.Name;
            DBActionStatus<UserGetUpdate> result = DBAccess.GetUser(currEmail);
            if (result.status == ResultType.Success)
            {
                return View(result.data);
            }
            else
            {
                DBAccess.WriteLog("Error", result.message, DateTime.Now);
                ModelState.AddModelError("", result.message);
                return View(new UserGetUpdate());
            }

        }

        [HttpPost]
        [Authorize]
        public ActionResult Update(UserGetUpdate model)
        {
            if (ModelState.IsValid)
            {
                string currEmail = User.Identity.Name;
                DBActionStatus<bool> result = DBAccess.UpdateUser(currEmail,model);

                if (result.status == ResultType.Success)
                {
                    DBAccess.WriteLog("Event", result.message, DateTime.Now);
                    return RedirectToAction("Get", "User");
                }
                else
                {
                    DBAccess.WriteLog("Error", result.message, DateTime.Now);
                    ModelState.AddModelError("", result.message);
                }
            }
            else
            {
                ModelState.AddModelError("", "Invalid data");
            }
            return View(model);
        }

    }
}