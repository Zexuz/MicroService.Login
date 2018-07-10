# MicroService.Login

A generic login system bulit with security as a focus.

## Backstory
This system was meant to be the the 3rd generation and the move from our PoC (Prof Of Concept) system into the enterprise stage. Sadly it was canceled before it could be completed. 

## Key notes
Password hashed with bCrypt
Security is done by passing JWT
Loggin HTTP request with the ELK stack.

## Fetures

* Login
* Register
* Email validation (Confirm email address before Login is enabled)
* Password reset 
* Enable 2FA with the Google Authentication App



### PS
This code has been reused,  and the product name has been replaced with `MicroService`.
Also, the `DomainValidation` part of the codebase is sometime going the be removed.
