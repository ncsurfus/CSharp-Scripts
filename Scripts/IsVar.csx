// Declare a variable in a conditional.
if("condition" is var conditionTest) {
    Console.WriteLine(conditionTest);
}

// It's also available here
Console.WriteLine(conditionTest);

// Pointlessly declare a variable in a conditional.
while("loop" is var loopTest)
{
    Console.WriteLine(loopTest);
    break;
}

// It's not available here
// Console.WriteLine(loopTest);