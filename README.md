# Introduction 
Simple web app with a couple of Rest APIs.

# How to run the app
Either build it and run it via the command line using regular dotnet commands (entry point is ```CheckoutPaymentGateway.dll```),
or build and run it through docker using the provided Dockerfile located at the project's root folder. The app will listen on
port 5000, which is configurable through the ```appsettings.json``` file. The log levels can be configured through
the same file. TLS is not supported on this first version. Some metrics are available on /metrics.

# APIs
* ```api/payments``` ```POST```: Process a payment. Need to provide the payment details in JSON format in the 
body of the request. Returns the ```TransactionId``` and the ```TransactionStatusCode``` from the bank. The
```TransactionStatusCode``` can have a value of 0 (successful) or 1 (unsuccessful). 

Example request body:

```JSON
{
	"CreditCardNumber": "4532-3214-9652-4199",
	"CreditCardExpiryMonth": 12,
	"CreditCardExpiryYear": 22,
	"CreditCardCvv": 345,
	"Amount": 1500,
	"Currency": "EUR"
}
```

Example response:

```JSON
{
    "transactionId": "48c32f52-fb0f-40c2-af47-cdef474ebbb8",
    "transactionStatusCode": 0
}
```

* ```api/payments/(transactionId)``` ```GET```: Get the details of a previously made payment. The 
```TransactionId``` needs to be provided in the URL of the request. Returns the payment information. 

Example response:

```JSON
{    
    "id": 1,
    "creditCardNumber": "************4199",
    "creditCardExpiryMonth": 12,
    "creditCardExpiryYear": 22,
    "creditCardCvv": 345,
    "amount": 1500,
    "currency": "EUR",
    "transactionId": "86bf5b18-48a8-42ec-aef2-b64f8ea9c7f2",
    "transactionStatusCode": 0
}
```

# Notes
I did not have enough time to polish the app. It's obviously not production ready, but I hope it will be enough
for this assignment. There are many things that need to be improved, like the unit tests (there are many edge cases
that aren't currently tested), and additions that need to be made, like adding TLS support. I've written my 
thoughts and things that I would change if I had more time in the code. The biggest concern is handling the 
credit card information. That should be the first thing to update, along with using proper storage instead of 
SqlLite in-memory.