﻿namespace UnitTests.Sequencer;

using Ieg.Sequencer;

public class SequenceEnumTests
{
    private const MyEnum InitialState = MyEnum.InitialState;

        
    public enum MyEnum
    {
        InitialState,
        State1,
        State2,
        StateX,
        Force,
    }


    [Theory]
    [InlineData(true, MyEnum.Force)]
    [InlineData(false, InitialState)]
    public void Test_ForceState(bool constraint, MyEnum expected)
    {
        var builder = SequenceBuilder.Configure(InitialState, builder =>
            builder.AddForceState(MyEnum.Force, () => constraint));

        var sut = builder.Build().Run();

        var actual = sut.CurrentState;
        Assert.Equal(expected.ToString(), actual);
    }

    [Theory]
    [InlineData(true, MyEnum.Force)]
    [InlineData(false, InitialState)]
    public void Test_ForceState_Only_Last_Counts(bool constraint, MyEnum expected)
    {
        var builder = SequenceBuilder.Configure(InitialState, builder =>
        {
            builder.AddForceState(MyEnum.StateX, () => constraint)
                .AddForceState(MyEnum.Force, () => constraint);
        });

        var sut = builder.Build().Run();

        var actual = sut.CurrentState;
        Assert.Equal(expected.ToString(), actual);
    }

    [Theory]
    [InlineData(true, MyEnum.State1)]
    [InlineData(false, InitialState)]
    public void Test_Set(bool constraint, MyEnum expected)
    {
        var builder = SequenceBuilder.Configure(InitialState, builder =>
            builder.AddForceState(MyEnum.Force, () => constraint));

        var sut = builder.Build();

        sut.SetState(MyEnum.State1, () => constraint);

        // no Execute is necessary

        var actual = sut.CurrentState;
        Assert.Equal(expected.ToString(), actual);
    }

    [Theory]
    [InlineData(true, MyEnum.State1)]
    [InlineData(false, InitialState)]
    public void Test_SetState_Only_Last_Counts(bool constraint, MyEnum expected)
    {
        var builder = SequenceBuilder.Configure(InitialState, builder =>
            builder.AddForceState(MyEnum.Force, () => constraint));

        var sut = builder.Build();
            
        sut.SetState(MyEnum.StateX, () => constraint);
        sut.SetState(MyEnum.State1, () => constraint);

        // no Execute is necessary

        var actual = sut.CurrentState;
        Assert.Equal(expected.ToString(), actual);
    }

    [Theory]
    [InlineData(MyEnum.State1, true,  MyEnum.State2)]
    [InlineData(MyEnum.State1, false, MyEnum.State1)]
    [InlineData(MyEnum.StateX, true,  MyEnum.StateX)]
    [InlineData(MyEnum.StateX, false, MyEnum.StateX)]
    public void Test_Constrain_Add_Conditional_State(MyEnum currentState, bool constraint, MyEnum expected)
    {
        var builder = SequenceBuilder.Configure(InitialState, builder =>
            builder.AddTransition(MyEnum.State1, MyEnum.State2, () => constraint));

        var sut = builder.Build();

        sut.SetState(currentState);
        sut.Run();

        var actual = sut.CurrentState;
        Assert.Equal(expected.ToString(), actual);
    }

    [Theory]
    [InlineData(MyEnum.State1, true, 1)]
    [InlineData(MyEnum.State1, false, 0)]
    [InlineData(MyEnum.StateX, true, 0)]
    [InlineData(MyEnum.StateX, false, 0)]
    public void Test_Action_Add_Conditional_State(MyEnum currentState, bool constraint, int expected)
    {
        var countStarts = 0;
        var builder = SequenceBuilder.Configure(InitialState, builder =>
            builder.AddTransition(MyEnum.State1, MyEnum.State2, () => constraint, () => countStarts = 1));

        var sut = builder.Build();

        sut.SetState(currentState);
        sut.Run();

        var actual = countStarts;
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(MyEnum.State1, true, 2)]
    public void Test_Concatenation_Add_Conditional_State(MyEnum currentState, bool constraint, int expected)
    {
        var countStarts = 0;
        var builder = SequenceBuilder.Configure(InitialState, builder =>
        {
            builder.AddTransition(MyEnum.State1, MyEnum.State2, () => constraint, () => countStarts++);
            builder.AddTransition(MyEnum.State2, MyEnum.StateX, () => constraint, () => countStarts++);
        });

        var sut = builder.Build();

        sut.SetState(currentState);
        sut.Run();

        var actualCount = countStarts;
        Assert.Equal(expected, actualCount);

        var actualState = sut.CurrentState;
        Assert.Equal(MyEnum.StateX.ToString(), actualState);
    }
}