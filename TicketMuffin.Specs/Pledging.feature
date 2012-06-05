Feature: Pledging
	In order to attend an event
	As a member of the public
	I want to pledge to attend an event

Scenario: Pledge to attend an event which is not full and sales are open
	Given Sales have not ended yet
	And The event is not full
	When I pledge to attend
	Then the pledge should be accepted

Scenario: Pledge to attend an event which is nearly ready to activate and sales are open
	Given Sales have not ended yet
	And The event needs one more pledge to be ready to activate
	When I pledge to attend
	And I complete the payment through paypal
	Then the event should be ready to activate

Scenario: Pledge to attend an event which is full
	Given The event is full
	When I pledge to attend
	Then the pledge should not be accepted with message "This event is full"

Scenario: Pledge to attend an event for which sales have ended
	Given Sales have ended
	When I pledge to attend
	Then the pledge should not be accepted with message "Sales for this event have ended"

Scenario: Pledge to attend an event which doesn't have enough spaces left
	Given Sales have not ended yet
	And The event has 2 spaces left
	When I pledge to attend
	Then the pledge should not be accepted with message "There are only 2 spaces left for this event"