﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NextGenSoftware.OASIS.API.Core.Helpers;
using NextGenSoftware.OASIS.API.ONode.Core.Managers;
using NextGenSoftware.OASIS.API.ONode.WebAPI.Interfaces;

namespace NextGenSoftware.OASIS.API.ONode.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NftController : OASISControllerBase
    {
        //private readonly INftService _nftService;

        //public NftController(INftService nftService)
        //{
        //    _nftService = nftService;
        //}

        public NftController()
        {
           
        }

        [HttpPost]
        [Route("CreateNftTransaction")]
        public async Task<OASISResult<NftTransactionRespone>> CreateNftTransaction(CreateNftTransactionRequest request)
        {
            return await NFTManager.Instance.CreateNftTransactionAsync(request);
        }

        //[HttpGet]
        //[Route("GetOLANDPrice")]
        //public async Task<OASISResult<int>> GetOlandPrice(int count, string couponCode)
        //{
        //    return await _nftService.GetOlandPrice(count, couponCode);
        //}

        //[HttpPost]
        //[Route("PurchaseOLAND")]
        //public async Task<OASISResult<PurchaseOlandResponse>> PurchaseOland(PurchaseOlandRequest request)
        //{
        //    return await _nftService.PurchaseOland(request);
        //}
    }
}