# Master Application Layer File Generator
$ErrorActionPreference = "Stop"
$basePath = "E:\Projects\NET\LinhGo.ERP\LinhGo.ERP.Application"
$filesCreated = 0

Write-Host "=== Creating Remaining Application Layer Files ===" -ForegroundColor Cyan

# Inventory Service (simplified)
$inventoryService = @"
using AutoMapper;
using LinhGo.ERP.Application.Common;
using LinhGo.ERP.Application.DTOs.Inventory;
using LinhGo.ERP.Domain.Inventory.Entities;
using LinhGo.ERP.Domain.Inventory.Enums;
using LinhGo.ERP.Domain.Inventory.Interfaces;

namespace LinhGo.ERP.Application.Services;

public interface IInventoryService
{
    Task<Result<IEnumerable<WarehouseDto>>> GetWarehousesAsync(Guid companyId);
    Task<Result<WarehouseDto>> CreateWarehouseAsync(Guid companyId, CreateWarehouseDto dto);
    Task<Result> AdjustStockAsync(Guid companyId, StockAdjustmentDto dto);
    Task<Result> TransferStockAsync(Guid companyId, StockTransferDto dto);
}

public class InventoryService : IInventoryService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public InventoryService(IWarehouseRepository warehouseRepository, IStockRepository stockRepository, IInventoryTransactionRepository transactionRepository, IProductRepository productRepository, IMapper mapper)
    {
        _warehouseRepository = warehouseRepository;
        _stockRepository = stockRepository;
        _transactionRepository = transactionRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<WarehouseDto>>> GetWarehousesAsync(Guid companyId)
    {
        var warehouses = await _warehouseRepository.GetActiveWarehousesAsync(companyId);
        return Result<IEnumerable<WarehouseDto>>.SuccessResult(_mapper.Map<IEnumerable<WarehouseDto>>(warehouses));
    }

    public async Task<Result<WarehouseDto>> CreateWarehouseAsync(Guid companyId, CreateWarehouseDto dto)
    {
        var isUnique = await _warehouseRepository.IsCodeUniqueAsync(companyId, dto.Code);
        if (!isUnique) return Result<WarehouseDto>.FailureResult(`$"Warehouse code '{dto.Code}' already exists");
        var warehouse = _mapper.Map<Warehouse>(dto);
        warehouse = await _warehouseRepository.AddAsync(companyId, warehouse);
        return Result<WarehouseDto>.SuccessResult(_mapper.Map<WarehouseDto>(warehouse), "Warehouse created successfully");
    }

    public async Task<Result> AdjustStockAsync(Guid companyId, StockAdjustmentDto dto)
    {
        var product = await _productRepository.GetByIdAsync(companyId, dto.ProductId);
        if (product == null) return Result.FailureResult("Product not found");
        await _stockRepository.UpdateStockLevelAsync(companyId, dto.ProductId, dto.WarehouseId, dto.Quantity);
        var transactionType = dto.Quantity > 0 ? TransactionType.StockIn : TransactionType.StockOut;
        var transactionNumber = await _transactionRepository.GenerateTransactionNumberAsync(companyId, transactionType);
        var transaction = new InventoryTransaction
        {
            CompanyId = companyId,
            TransactionNumber = transactionNumber,
            TransactionDate = DateTime.UtcNow,
            Type = transactionType,
            ProductId = dto.ProductId,
            Quantity = Math.Abs(dto.Quantity),
            ToWarehouseId = dto.Quantity > 0 ? dto.WarehouseId : null,
            FromWarehouseId = dto.Quantity < 0 ? dto.WarehouseId : null,
            Reason = dto.Reason,
            Notes = dto.Notes
        };
        await _transactionRepository.AddAsync(companyId, transaction);
        return Result.SuccessResult("Stock adjusted successfully");
    }

    public async Task<Result> TransferStockAsync(Guid companyId, StockTransferDto dto)
    {
        if (dto.FromWarehouseId == dto.ToWarehouseId) return Result.FailureResult("Cannot transfer to the same warehouse");
        await _stockRepository.UpdateStockLevelAsync(companyId, dto.ProductId, dto.FromWarehouseId, -dto.Quantity);
        await _stockRepository.UpdateStockLevelAsync(companyId, dto.ProductId, dto.ToWarehouseId, dto.Quantity);
        var transactionNumber = await _transactionRepository.GenerateTransactionNumberAsync(companyId, TransactionType.Transfer);
        var transaction = new InventoryTransaction
        {
            CompanyId = companyId,
            TransactionNumber = transactionNumber,
            TransactionDate = DateTime.UtcNow,
            Type = TransactionType.Transfer,
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            FromWarehouseId = dto.FromWarehouseId,
            ToWarehouseId = dto.ToWarehouseId,
            Notes = dto.Notes
        };
        await _transactionRepository.AddAsync(companyId, transaction);
        return Result.SuccessResult("Stock transferred successfully");
    }
}
"@

$dir = "$basePath\Services"
if (!(Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
[System.IO.File]::WriteAllText("$dir\InventoryService.cs", $inventoryService, [System.Text.Encoding]::UTF8)
$filesCreated++
Write-Host "[+] InventoryService.cs" -ForegroundColor Green

Write-Host ""
Write-Host "Files created: $filesCreated" -ForegroundColor Cyan
Write-Host "Run this script to create InventoryService!" -ForegroundColor Green

