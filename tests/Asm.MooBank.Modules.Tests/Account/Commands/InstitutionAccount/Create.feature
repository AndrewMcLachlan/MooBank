Feature: Create Account

As a user,I want to create an account so that I can manage my money.

Scenario: Create Account
Given I have a request to create an institution account
When I call CreateHandler.Handle
Then the account is created

Scenario: Create Account with invalid account group ID
Given I have a request to create an institution account
And I have an invalid group ID
When I call CreateHandler.Handle
Then an exception of type 'Asm.NotAuthorisedException, Asm' is thrown