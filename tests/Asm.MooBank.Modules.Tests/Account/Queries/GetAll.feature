Feature: Get all accounts

@Unit
Scenario: Get all accounts
Given I have the set of generated accounts
When I call GetAllHandler.Handle
Then the accounts with the following Ids are returned
| Id                                   |
| f1b1b1b1-1b1b-1b1b-1b1b-1b1b1b1b1b1b |
| 8286d046-9740-a3e4-95cf-ff46699c73c4 |
| 3410cda1-5b13-a34e-6f84-a54adf7a0ea0 |
| 17c1e39c-e46c-828e-4e02-21be0f3b5358 |
| 1828a3f0-e94b-eb91-81e3-12bcf19ac41a |
| 86a95ec1-5bda-bd57-3f8e-e610398f9334 |
| 3c5a4d30-f652-b355-51d6-9589d1d290b4 |
