﻿using AutoMapper;
using E_Commerce.APIs.DTO;
using E_Commerce.Core.Entities;
using E_Commerce.Core.Repositories.Contract;
using E_Commerce.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.APIs.Controllers
{

    public class CartController : BaseApiController
    {
        private readonly ICartRepositery cartRepositery;
        IMapper _mapper;
        IUnitOfWork unit;
        public CartController(ICartRepositery cartRepo,IMapper mapper, IUnitOfWork unit)
        {
            cartRepositery = cartRepo;
            _mapper = mapper;
            this.unit = unit;

        }
        [HttpGet("{id}")]
        public async Task<ActionResult<customerCart>>getCart(string id)
        {
            var cart=await cartRepositery.getCartAsync(id);
            if(cart == null)
            {
                cart = new customerCart(id);
                var sevedCart=await cartRepositery.UpdateCartAsync(cart);
                return Ok (sevedCart);
             
            }
            return Ok(cart);
        }
        [HttpPost]
        public async Task<ActionResult<customerCart>>updateCart(CustomerCartDto cart)
        {
           var MappedCart= _mapper.Map<CustomerCartDto, customerCart>(cart);
            var createdOrUpdatedCart= await cartRepositery.UpdateCartAsync(MappedCart);
            if(createdOrUpdatedCart == null)
            {
                return BadRequest(new ApiResponse(400));
            }
            return Ok(createdOrUpdatedCart);
        }

        [HttpDelete]
        public async Task DeleteCart(string id)
        {
            await cartRepositery.deleteCartAsync(id);
        }
        



    }
}
