/*
 Pre-Deployment Script
 Runs before the schema comparison's statements, but note that the comparison plans those statements
 against the database as it stands *before* this script runs. So this can prepare data the plan is
 about to move or constrain -- staging rows out of a table it will rebuild, backfilling a column it
 is about to make NOT NULL -- but it cannot create or drop an object the plan has already decided to
 create or drop.

 One-off migrations live here only until every environment has run them; they are removed once
 spent.
*/
