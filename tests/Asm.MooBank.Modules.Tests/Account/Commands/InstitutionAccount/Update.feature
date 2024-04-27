Feature: Update Account

As a user, I want to update an account so that I can manage my money.

Scenario: Update Account
Given I have a request to update an institution account
When I call UpdateHandler.Handle
Then the account is updated

Scenario: Update Account with invalid account ID
Given I have a request to update an institution account
And I have an invalid account ID
When I call UpdateHandler.Handle
Then an exception of type 'Asm.NotAuthorisedException, Asm' is thrown

Scenario: Update Account with invalid account group ID
Given I have a request to update an institution account
And I have an invalid group ID
When I call UpdateHandler.Handle
Then an exception of type 'Asm.NotAuthorisedException, Asm' is thrown