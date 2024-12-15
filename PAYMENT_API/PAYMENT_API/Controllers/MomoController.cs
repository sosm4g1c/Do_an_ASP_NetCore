using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace PAYMENT_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MomoController : ControllerBase
    {
        [HttpGet("query-status")]
        public async Task<IActionResult> QueryOrderStatus(string orderId)
        {
            // Parameters
            var accessKey = "F8BBA842ECF85";
            var secretKey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";
            var partnerCode = "MOMO";
            var requestId = orderId; // Hoặc tạo mới nếu cần

            // Generate raw signature
            var rawSignature = $"accessKey={accessKey}&orderId={orderId}&partnerCode={partnerCode}&requestId={requestId}";

            Console.WriteLine("--------------------RAW SIGNATURE----------------");
            Console.WriteLine(rawSignature);

            // Generate signature
            var signature = GenerateSignature(rawSignature, secretKey);

            Console.WriteLine("--------------------SIGNATURE----------------");
            Console.WriteLine(signature);

            // JSON request body
            var requestBody = new
            {
                partnerCode,
                accessKey,
                requestId,
                orderId,
                signature
            };

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);

            // Send HTTP POST request
            var response = await SendRequestToMomoQuery(jsonRequestBody);

            // Return response to client
            return Ok(response);
        }

        // Helper method to send request to MoMo Query API
        private static async Task<string> SendRequestToMomoQuery(string requestBody)
        {
            var endpointUrl = "https://test-payment.momo.vn/v2/gateway/api/query";

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(endpointUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Response Body:");
                    Console.WriteLine(responseBody);
                    return responseBody;
                }
                else
                {
                    return JsonConvert.SerializeObject(new
                    {
                        error = true,
                        message = response.ReasonPhrase,
                        statusCode = response.StatusCode
                    });
                }
            }
        }


        [HttpPost("create-transaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            // Parameters
            var accessKey = "F8BBA842ECF85";
            var secretKey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";
            var partnerCode = "MOMO";
            var orderInfo = "pay with MoMo";
            var redirectUrl = request.RedirectUrl;
            var ipnUrl = request.IpnUrl;
            var requestType = "payWithMethod";
            var amount = request.Amount.ToString();
            var orderId = partnerCode + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var requestId = orderId;
            var extraData = "";
            var orderGroupId = "";
            var autoCapture = true;
            var lang = "vi";

            // Create raw signature
            var rawSignature = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";

            Console.WriteLine("--------------------RAW SIGNATURE----------------");
            Console.WriteLine(rawSignature);

            // Generate HMAC SHA256 signature
            var signature = GenerateSignature(rawSignature, secretKey);

            Console.WriteLine("--------------------SIGNATURE----------------");
            Console.WriteLine(signature);

            // JSON request body
            var requestBody = new
            {
                partnerCode,
                partnerName = "Test",
                storeId = "MomoTestStore",
                requestId,
                amount,
                orderId,
                orderInfo,
                redirectUrl,
                ipnUrl,
                lang,
                requestType,
                autoCapture,
                extraData,
                orderGroupId,
                signature
            };

            var jsonRequestBody = JsonConvert.SerializeObject(requestBody);

            // Send HTTP POST request
            var response = await SendRequestToMomo(jsonRequestBody);

            // Return the response
            return Ok(response);
        }

        // Generate HMAC SHA256 signature
        private static string GenerateSignature(string rawData, string secretKey)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        // Send POST request to MoMo API
        private static async Task<string> SendRequestToMomo(string requestBody)
        {
            var endpointUrl = "https://test-payment.momo.vn/v2/gateway/api/create";

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(endpointUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Response Body:");
                    Console.WriteLine(responseBody);
                    return responseBody;
                }
                else
                {
                    return JsonConvert.SerializeObject(new
                    {
                        error = true,
                        message = response.ReasonPhrase,
                        statusCode = response.StatusCode
                    });
                }
            }
        }
    }

    // CreateTransactionRequest class to capture POST data
    public class CreateTransactionRequest
    {
        public string RedirectUrl { get; set; }
        public string IpnUrl { get; set; }
        public decimal Amount { get; set; }
    }
}
