# Yepe
** I didn't include any authorization mechanism since this is a challenge. I assumed I was working in a secure environment, allowing me to focus solely on functionality.

### Access the API

Once the application is running, you can access the API at `https://localhost:****/swagger/index.html` to view the Swagger UI and interact with the endpoints.

## Transaction Flow

### Create a Transaction

1. **Endpoint**: `POST /Transaction`
2. **Request Body**: `CreateTransactionDTO`
   - `SourceAccountId` (Guid): The ID of the source account.
   - `TargetAccountId` (Guid): The ID of the target account.
   - `TransferTypeId` (TransferType): The type of transfer (Internal or External).
   - `Value` (decimal): The amount to be transferred.
3. **Response**: 
   - `201 Created`: The transaction was successfully created. Return the transaction Id (Guid).
   - `400 Bad Request`: The request was invalid.

### Retrieve a Transaction

1. **Endpoint**: `GET /Transaction`
2. **Query Parameters**:
   - `transactionId` (Guid): The unique identifier of the transaction.
   - `createdDate` (DateOnly): The date the transaction was created (MM-dd-yyyy).
3. **Response**:
   - `200 OK`: The transaction was found and returned. Return a Transaction entity.
   - `400 Bad Request`: The request was invalid.
   - `404 Not Found`: The transaction was not found.

## Project Structure

- `transaction-service.api`: Contains the API controllers and startup configuration.
- `transaction-service.application`: Contains the application services, business interfaces and DTOs.
- `transaction-service.domain`: Contains the domain entities and interfaces.
- `transaction-service.infrastructure`: Contains the infrastructure implementations.
- `transaction-service.worker`: Contains the background services.

## Antifraud-service Processing Flow

### Background Service

The antifraud service runs as a background worker that listens to Kafka topics for transaction messages. It processes each message to detect potential fraud based on predefined rules and criteria.

## Project Structure

- `antifraud-service.api`: Contains the API controllers and startup configuration.
- `antifraud-service.application`: Contains the application services and business interfaces.
- `antifraud-service.domain`: Contains the domain entities and interfaces.
- `antifraud-service.infrastructure`: Contains the infrastructure implementations.
- `antifraud-service.worker`: Contains the background services.

## General FLOW 

- transaction-service  
1) POST /Transaction -> (via a DTO) creates a transaction with a pending status.  
2) Inserts the transaction into MongoDB, within the transactions collection inside the transaction_service database.  
3) Publishes a TransactionCreatedEvent to Kafka, under the topic "transaction-created".  

- antifraud-service  
1) TransactionCreatedConsumer listens for the event type TransactionCreatedEvent.  
2) TransactionCreatedConsumer processes the event and applies validation, approving or rejecting the transaction.  
3) TransactionAntiFraudProducer publishes an event type TransactionStatusUpdateEvent to the topic "transaction-updated".  
4) If the transaction was approved, it save(or update)  a record with the amount into the antifraud_service database for future validation purposes.  

 - transaction-service (back)  
1) TransactionUpdateConsumer listens for the event type TransactionStatusUpdateEvent.  
2) Receives the event and updates the corresponding transaction status accordingly into the db for the corresponding  transaction .  

### Access Kafka UI

Once the application is running, you can access the Kafka UI at `http://localhost:8080` to monitor Kafka topics and messages.


### Kafka Configuration

The service is configured to connect to a Kafka broker and listen to specific topics. The configuration settings are provided through environment variables:

- `KafkaSettings__BootstrapServers`: The Kafka broker address (e.g., `kafka:9092`).


## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## License

This project is licensed under the MIT License.
# This .gitignore file was automatically created by Microsoft(R) Visual Studio.