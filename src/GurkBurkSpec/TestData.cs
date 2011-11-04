namespace GurkBurkSpec
{
    public class TestData
    {
        public const string AcceptanceTest = @"
@tag1 @tag2
Feature: foo
	As a
	I want
	So that

@tag3
Scenario: x
Given a
	b
	c
When d
Then e
  | x | y | z |
  | 1 | 2 | 3 |";

    }
}