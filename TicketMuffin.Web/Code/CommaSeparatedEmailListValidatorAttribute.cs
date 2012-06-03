using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace GroupGiving.Web.Code
{
    public class CommaSeparatedEmailListValidatorAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value==null)
            {
                return true;
            }

            string emails = (string) value;
            string[] separated = emails.Split(',');

            if (separated.Length==0)
            {
                return true;
            }

            foreach(var email in separated)
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    continue;
                }

                try
                {
                    var address = new MailAddress(email);
                } catch(ArgumentException)
                {
                    return false;
                } catch (FormatException)
                {
                    return false;
                }
            }

            return true;
        }
    }
}