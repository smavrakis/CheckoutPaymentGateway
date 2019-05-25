# Introduction 
Simple web app with a couple of Rest APIs.

# How to run the app
Either build it and run it via the command line using regular dotnet commands, or build and run it through docker
using the provided Dockerfile located at the project's root folder.

# Notes
I did not have enough time to polish the app. It's obviously not production ready, but I hope it will be enough
for this assignment. There are many things that need to be improved, like the unit tests. I've written my thoughts 
and things that I would change if I had more time in the code. The biggest concern is handling the credit card 
information. That should be the first thing to update, along with using proper storage instead of SqlLite in-memory.