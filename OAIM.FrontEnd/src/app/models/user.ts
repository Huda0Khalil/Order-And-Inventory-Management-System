export interface User {
    userName:string;
    email:string;
    phoneNumber: string;
    tenantId :string;
    password:string;
    confirmPassword:string;
    role?:string;
}
/*
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string TenantId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        //[Required]
        public string Role { get; set; }
    }
}
 */