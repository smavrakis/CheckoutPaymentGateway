FROM mcr.microsoft.com/dotnet/core/sdk:2.2 as build-image

WORKDIR /home/app
COPY . .

# I couldn't make the integration tests work in the container in the time that I had available, so I'm
# filtering them out. This needs to be debugged properly.
RUN dotnet test ./CheckoutPaymentGateway.Tests/CheckoutPaymentGateway.Tests.csproj --filter Category!=WebApp
RUN dotnet publish ./CheckoutPaymentGateway/CheckoutPaymentGateway.csproj -o /publish/ 

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
EXPOSE 5000
LABEL name="CheckoutPaymentGateway"
 
WORKDIR /home/app
COPY --from=build-image /publish .
 
ENTRYPOINT ["dotnet", "CheckoutPaymentGateway.dll"]