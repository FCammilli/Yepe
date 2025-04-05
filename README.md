# Yepe
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

## Antifraud Processing Flow

### Background Service

The antifraud service runs as a background worker that listens to Kafka topics for transaction messages. It processes each message to detect potential fraud based on predefined rules and criteria.

## Project Structure

- `antifraud-service.api`: Contains the API controllers and startup configuration.
- `antifraud-service.application`: Contains the application services and business interfaces.
- `antifraud-service.domain`: Contains the domain entities and interfaces.
- `antifraud-service.infrastructure`: Contains the infrastructure implementations.
- `antifraud-service.worker`: Contains the background services.

This command will build the Docker images and start the containers defined in the `docker-compose.yml` file.

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