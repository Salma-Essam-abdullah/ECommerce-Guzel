﻿using E_Commerce.Core.Entities;
using E_Commerce.Core.Entities.Oreder_Agrigate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerce.Core.Services.Contract
{
    public interface IPaymentService
    {
        Task<customerCart> CreateOrUpdatePaymentIntent(string cartid);
       Task<Order> UpdatePaymentIntentToSuccededOrFailed(string paymentIntentId, bool isSucceded);
    }
}
