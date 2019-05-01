using System;

namespace AKSoftware.WebApi.Client
{
    internal class RegisterBindingModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
