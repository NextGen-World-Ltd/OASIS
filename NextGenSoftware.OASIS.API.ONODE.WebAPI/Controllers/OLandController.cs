﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NextGenSoftware.OASIS.API.Core.Helpers;
using NextGenSoftware.OASIS.API.ONode.WebAPI.Interfaces;

namespace NextGenSoftware.OASIS.API.ONode.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OLandController : OASISControllerBase
    {
        //private readonly INftService _nftService;
        //public OLandController(INftService nftService)
        //{
        //    _nftService = nftService;
        //}

        public OLandController()
        {

        }

        [HttpPost]
        [Route("CreatePurchase")]
        public async Task<OASISResult<NftTransactionRespone>> CreateNftTransaction(CreateNftTransactionRequest request)
        {
            return await _nftService.CreateNftTransaction(request);
        }

        [HttpGet]
        [Route("GetOLANDPrice")]
        public async Task<OASISResult<int>> GetOlandPrice(int count, string couponCode)
        {
            return await _nftService.GetOlandPrice(count, couponCode);
        }

        [HttpPost]
        [Route("PurchaseOLAND")]
        public async Task<OASISResult<PurchaseOlandResponse>> PurchaseOland(PurchaseOlandRequest request)
        {
            return await _nftService.PurchaseOland(request);
        }
    }
}