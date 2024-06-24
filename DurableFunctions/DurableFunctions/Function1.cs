using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace DurableFunction
{
    public static class OrderFunction
    {
        [Function(nameof(OrderOrchestrator))]
        public static async Task OrderOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var orderId = context.GetInput<int>();

            // Step 1: Update Inventory
            await context.CallActivityAsync(nameof(UpdateInventory), orderId);

            // Step 2: Process Payment
            await context.CallActivityAsync(nameof(ProcessPayment), orderId);

            // Step 3: Confirm Order
            await context.CallActivityAsync(nameof(ConfirmOrder), orderId);

            // Step 4: Notify User
            await context.CallSubOrchestratorAsync(nameof(NotificationOrchestrator), orderId);
        }

        [Function(nameof(UpdateInventory))]
        public static async Task UpdateInventory([ActivityTrigger] int orderId, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UpdateInventory");
            logger.LogInformation($"Updating inventory for order {orderId}.");

            // Implement your inventory update logic here
            // For example, reduce the stock quantity in the database
            // Assuming a placeholder implementation
            await Task.Delay(500); // Simulate some processing time

            await Task.CompletedTask;
        }

        [Function(nameof(ProcessPayment))]
        public static async Task ProcessPayment([ActivityTrigger] int orderId, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ProcessPayment");
            logger.LogInformation($"Processing payment for order {orderId}.");

            // Implement your payment processing logic here
            // For example, interact with a payment gateway API
            await Task.Delay(500); // Simulate some processing time

            await Task.CompletedTask;
        }

        [Function(nameof(ConfirmOrder))]
        public static async Task ConfirmOrder([ActivityTrigger] int orderId, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ConfirmOrder");
            logger.LogInformation($"Confirming order {orderId}.");

            // Implement your order confirmation logic here
            // For example, mark the order as confirmed in the database
            await Task.Delay(500); // Simulate some processing time

            await Task.CompletedTask;
        }

        [Function(nameof(NotificationOrchestrator))]
        public static async Task NotificationOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var orderId = context.GetInput<int>();

            // Step 1: Send Order Received Notification
            await context.CallActivityAsync(nameof(SendOrderReceivedNotification), orderId);

            // Step 2: Send Payment Processed Notification
            await context.CallActivityAsync(nameof(SendPaymentProcessedNotification), orderId);

            // Step 3: Send Order Confirmed Notification
            await context.CallActivityAsync(nameof(SendOrderConfirmedNotification), orderId);
        }

        [Function(nameof(SendOrderReceivedNotification))]
        public static async Task SendOrderReceivedNotification([ActivityTrigger] int orderId, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SendOrderReceivedNotification");
            logger.LogInformation($"Sending order received notification for order {orderId}.");

            // Implement your notification logic here
            // For example, send an email or push notification
            await Task.Delay(500); // Simulate some processing time

            await Task.CompletedTask;
        }

        [Function(nameof(SendPaymentProcessedNotification))]
        public static async Task SendPaymentProcessedNotification([ActivityTrigger] int orderId, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SendPaymentProcessedNotification");
            logger.LogInformation($"Sending payment processed notification for order {orderId}.");

            // Implement your notification logic here
            // For example, send an email or push notification
            await Task.Delay(500); // Simulate some processing time

            await Task.CompletedTask;
        }

        [Function(nameof(SendOrderConfirmedNotification))]
        public static async Task SendOrderConfirmedNotification([ActivityTrigger] int orderId, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("SendOrderConfirmedNotification");
            logger.LogInformation($"Sending order confirmed notification for order {orderId}.");

            // Implement your notification logic here
            // For example, send an email or push notification
            await Task.Delay(500); // Simulate some processing time

            await Task.CompletedTask;
        }

        [Function("OrderOrchestrator_HttpStart")]
        public static async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("OrderOrchestrator_HttpStart");
            var requestBody = await req.ReadAsStringAsync();
            var orderId = int.Parse(requestBody);

            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(OrderOrchestrator), orderId);

            logger.LogInformation($"Started order orchestration with ID = '{instanceId}' for order ID = '{orderId}'.");

            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }
}