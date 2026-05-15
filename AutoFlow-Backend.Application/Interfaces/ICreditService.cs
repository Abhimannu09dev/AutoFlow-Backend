using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Credits;

namespace AutoFlow_Backend.Application.Interfaces;

public interface ICreditService
{
    Task<ApiResponse<CreditDetailResponse>> GetCreditDetailsAsync(Guid saleId, CancellationToken cancellationToken = default);
    Task<ApiResponse<RecordCreditPaymentResponse>> RecordPaymentAsync(Guid saleId, RecordCreditPaymentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateCreditStatusResponse>> UpdateStatusAsync(Guid saleId, UpdateCreditStatusRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<SendCreditReminderResponse>> SendReminderAsync(Guid saleId, CancellationToken cancellationToken = default);
}
