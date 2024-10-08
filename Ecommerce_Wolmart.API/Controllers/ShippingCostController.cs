﻿using AutoMapper;
using Contracts;
using Entities.Models.Address;
using Microsoft.AspNetCore.Mvc;
using Shared.DTO.Contact;
using Shared.DTO.Product;
using Shared.DTO.Response;
using Shared.DTO.ShippingCost;

namespace Ecommerce_Wolmart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShippingCostController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;

        public ShippingCostController(ILoggerManager logger, IRepositoryManager repository, IMapper mapper)
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetShippingCostByAddressId/{addressId}")]
        public async Task<IActionResult> GetShippingCostByAddressId(Guid addressId)
        {
            //Lấy id địa chỉ
            var address = await _repository.Address.GetAddressByIdAsync(addressId, trackChanges: false);
            if (address == null)
            {
                return NotFound(new ApiResponse<Object>
                {
                    Success = false,
                    Message = $"Không tìm thấy địa chỉ này {addressId}",
                    Data = null
                });
            }

            // Lấy ProvinceCode từ địa chỉ
            string provinceCode = address.ProvinceCode;
            string provinceName = address.ProvinceName;

            //Lấy phí vận chuyển dựa trên ProvinceCode 
            var shippingCost = await _repository.ShippingCost.GetShippingCostByProvinceAsync(provinceCode);
            if (shippingCost == null)
            {
                return NotFound(new ApiResponse<Object>
                {
                    Success = false,
                    Message = $"Không tìm thấy chi phí vận chuyển {provinceCode}",
                    Data = null
                });
            }
            var shippingCostDto = new ShippingCostDto
            {
                Id = shippingCost.Id,
                ProvinceCode = shippingCost.ProvinceCode,
                ProvinceName = provinceName, // Thêm ProvinceName
                Cost = shippingCost.Cost
            };
            //Chuyển đổi model thành DTO
            return Ok(new ApiResponse<ShippingCostDto>
            {
                Success = true,
                Message = "Address retrieved successfully.",
                Data = shippingCostDto
            });
        }

        [HttpPut]
        [Route("UpdateShippingCost/{Id}")]
        public async Task<IActionResult> UpdateShippingCost(Guid Id, [FromBody] UpdateCostDto updateCostDto)
        {
            try
            {
                if (updateCostDto == null)
                {
                    return NotFound(new ApiResponse<Object>
                    {
                        Success = false,
                        Message = $"Không tìm thấy id này {Id}",
                        Data = null
                    });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid ShippingCost object sent from client.");
                    return NotFound(new ApiResponse<Object>
                    {
                        Success = false,
                        Message = $"Invalid model object.",
                        Data = null
                    });
                }

                var costEntity = await _repository.ShippingCost.GetShippingCostByIdAsync(Id, trackChanges: true);
                if (costEntity == null)
                {
                    _logger.LogError($"ShippingCost with id: {Id}, hasn't been found in db.");
                    return NotFound(new ApiResponse<Object>
                    {
                        Success = false,
                        Message = $"Invalid model object.",
                        Data = null
                    });
                }

                _mapper.Map(updateCostDto, costEntity);

                _repository.ShippingCost.UpdateShippingCost(costEntity);
                _repository.SaveAsync();

                return Ok(_mapper.Map<ShippingCostDto>(costEntity));
            }
            catch (Exception ex)
            {

                _logger.LogError($"Something went wrong inside ShippingCost action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("GetAllShippingCost")]
        public async Task<IActionResult> GetAllShippingCost()
        {
            try
            {
                // Lấy tất cả sản phẩm có IsHot là true
                var shippingCosts = await _repository.ShippingCost.GetAllShippingCostAsync(trackChanges: false);

                if (shippingCosts == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Không tìm thấy phí vận chuyển",
                        Data = null
                    });
                }

                // Ánh xạ từ entity sang DTO nếu cần thiết
                var shippingCostsDto = _mapper.Map<IEnumerable<ShippingCostDto>>(shippingCosts);

                return Ok(new ApiResponse<IEnumerable<ShippingCostDto>>
                {
                    Success = true,
                    Message = "Danh sách phí vận chuyển !.",
                    Data = shippingCostsDto
                });
            }
            catch (Exception ex)
            {

                _logger.LogError($"Something went wrong inside GetAllProductIsHot action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
