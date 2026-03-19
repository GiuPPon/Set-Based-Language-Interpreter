open Semantics

(* some tests with the static semantics *)

(* success/prog01.txt *)
let prog1 =
    LangProg(
        NonEmptyStmtSeq(
            PrintStmt(Union(Union(SetLit(IntLiteral 1), SetLit(IntLiteral 2)), SetLit(IntLiteral 3))),
            EmptyStmtSeq
        )
    )

typecheckProg prog1
executeProg prog1

(* success/prog02.txt *)

let prog2 =
    LangProg(
        NonEmptyStmtSeq(
            VarStmt(Name "s1", Union(Union(SetLit(IntLiteral 1), SetLit(IntLiteral 2)), SetLit(IntLiteral 3))),
            NonEmptyStmtSeq(
                VarStmt(Name "s2", SetEnum(Name "x", Variable(Name "s1"), Variable(Name "x"))),
                NonEmptyStmtSeq(AssertStmt(Eq(Variable(Name "s1"), Variable(Name "s2"))), EmptyStmtSeq)
            )
        )
    )

typecheckProg prog2
executeProg prog2

(* success/prog03.txt *)

let prog3 =
    LangProg(
        NonEmptyStmtSeq(
            VarStmt(Name "s1", Union(Union(SetLit(IntLiteral 1), SetLit(IntLiteral 2)), SetLit(IntLiteral 3))),
            NonEmptyStmtSeq(
                AssertStmt(
                    Not(
                        Eq(
                            Diff(Union(Variable(Name "s1"), Variable(Name "s1")), Variable(Name "s1")),
                            Union(Variable(Name "s1"), Diff(Variable(Name "s1"), Variable(Name "s1")))
                        )
                    )
                ),
                EmptyStmtSeq
            )
        )
    )


typecheckProg prog3
executeProg prog3

(* success/prog04.txt *)

let prog4 =
    LangProg(
        NonEmptyStmtSeq(
            VarStmt(Name "s1", SetLit(IntLiteral 1)),
            NonEmptyStmtSeq(
                AssertStmt(
                    And(
                        And(
                            And(
                                IsIn(IntLiteral 1, Variable(Name "s1")),
                                IsIn(IntLiteral 2, Union(Variable(Name "s1"), SetLit(IntLiteral 2)))
                            ),
                            IsIn(IntLiteral 1, Diff(Variable(Name "s1"), SetLit(IntLiteral 2)))
                        ),
                        Eq(IsIn(IntLiteral 2, Variable(Name "s1")), IsIn(IntLiteral 3, Variable(Name "s1")))
                    )
                ),
                EmptyStmtSeq
            )
        )
    )

typecheckProg prog4
executeProg prog4

(* success/prog05.txt *)

let prog5 =
    LangProg(
        NonEmptyStmtSeq(
            AssertStmt(IsIn(Add(Mul(IntLiteral 11, IntLiteral 2), IntLiteral 2), SetLit(IntLiteral 24))),
            EmptyStmtSeq
        )
    )


typecheckProg prog5
executeProg prog5

(* success/prog06.txt *)

let prog6 =
    LangProg(
        NonEmptyStmtSeq(
            VarStmt(Name "s1", Union(Union(SetLit(IntLiteral 1), SetLit(IntLiteral 2)), SetLit(IntLiteral 3))),
            NonEmptyStmtSeq(
                VarStmt(Name "s2", Union(Union(SetLit(IntLiteral 3), SetLit(IntLiteral 4)), SetLit(IntLiteral 5))),
                NonEmptyStmtSeq(
                    AssertStmt(
                        Eq(
                            Size(Union(Variable(Name "s1"), Variable(Name "s2"))),
                            Add(Add(Size(Variable(Name "s1")), Size(Variable(Name "s2"))), Minus(IntLiteral 1))
                        )
                    ),
                    EmptyStmtSeq
                )
            )
        )
    )

typecheckProg prog6
executeProg prog6

(* success/prog07.txt *)

let prog7 =
    LangProg(
        NonEmptyStmtSeq(
            VarStmt(Name "s1", Union(Union(SetLit(IntLiteral 1), SetLit(IntLiteral 2)), SetLit(IntLiteral 3))),
            NonEmptyStmtSeq(
                VarStmt(
                    Name "s2",
                    Union(
                        Union(
                            SetEnum(
                                Name "x",
                                Variable(Name "s1"),
                                Add(Mul(IntLiteral 3, Variable(Name "x")), IntLiteral 1)
                            ),
                            SetEnum(
                                Name "x",
                                Variable(Name "s1"),
                                Add(Mul(IntLiteral 3, Variable(Name "x")), IntLiteral 2)
                            )
                        ),
                        SetEnum(Name "x", Variable(Name "s1"), Add(Mul(IntLiteral 3, Variable(Name "x")), IntLiteral 3))
                    )
                ),
                NonEmptyStmtSeq(
                    AssertStmt(Eq(Size(Variable(Name "s2")), IntLiteral 9)),
                    NonEmptyStmtSeq(
                        VarStmt(Name "i", IntLiteral 4),
                        NonEmptyStmtSeq(
                            WhileStmt(
                                Not(Eq(Variable(Name "i"), IntLiteral 13)),
                                Block(
                                    NonEmptyStmtSeq(
                                        AssertStmt(IsIn(Variable(Name "i"), Variable(Name "s2"))),
                                        NonEmptyStmtSeq(
                                            AssignStmt(Name "i", Add(Variable(Name "i"), IntLiteral 1)),
                                            EmptyStmtSeq
                                        )
                                    )
                                )
                            ),
                            EmptyStmtSeq
                        )
                    )
                )
            )
        )
    )

typecheckProg prog7
executeProg prog7

(* success/prog08.txt *)

let prog8 =
    LangProg(
        NonEmptyStmtSeq(
            VarStmt(Name "i", IntLiteral 0),
            NonEmptyStmtSeq(
                VarStmt(Name "s", Diff(SetLit(IntLiteral 0), SetLit(IntLiteral 0))),
                NonEmptyStmtSeq(
                    WhileStmt(
                        Not(Eq(Variable(Name "i"), IntLiteral 10)),
                        Block(
                            NonEmptyStmtSeq(
                                AssignStmt(Name "s", Union(Variable(Name "s"), SetLit(Variable(Name "i")))),
                                NonEmptyStmtSeq(
                                    AssignStmt(Name "i", Add(Variable(Name "i"), IntLiteral 1)),
                                    EmptyStmtSeq
                                )
                            )
                        )
                    ),
                    NonEmptyStmtSeq(
                        AssertStmt(Eq(Size(Variable(Name "s")), IntLiteral 10)),
                        NonEmptyStmtSeq(
                            AssignStmt(Name "i", IntLiteral 0),
                            NonEmptyStmtSeq(
                                WhileStmt(
                                    Not(Eq(Variable(Name "i"), IntLiteral 10)),
                                    Block(
                                        NonEmptyStmtSeq(
                                            AssertStmt(IsIn(Variable(Name "i"), Variable(Name "s"))),
                                            NonEmptyStmtSeq(
                                                AssignStmt(Name "i", Add(Variable(Name "i"), IntLiteral 1)),
                                                EmptyStmtSeq
                                            )
                                        )
                                    )
                                ),
                                EmptyStmtSeq
                            )
                        )
                    )
                )
            )
        )
    )


typecheckProg prog8
executeProg prog8

(* success/prog09.txt *)

let prog9 =
    LangProg(
        NonEmptyStmtSeq(
            VarStmt(
                Name "s1",
                Union(SetLit(PairLit(IntLiteral 1, BoolLiteral true)), SetLit(PairLit(IntLiteral 2, BoolLiteral false)))
            ),
            NonEmptyStmtSeq(
                AssertStmt(Eq(Size(Variable(Name "s1")), IntLiteral 2)),
                NonEmptyStmtSeq(
                    AssertStmt(IsIn(PairLit(IntLiteral 1, BoolLiteral true), Variable(Name "s1"))),
                    NonEmptyStmtSeq(
                        VarStmt(
                            Name "s2",
                            Union(
                                SetLit(Union(Union(SetLit(IntLiteral 1), SetLit(IntLiteral 2)), SetLit(IntLiteral 3))),
                                SetLit(Union(Union(SetLit(IntLiteral 3), SetLit(IntLiteral 4)), SetLit(IntLiteral 5)))
                            )
                        ),
                        NonEmptyStmtSeq(
                            AssertStmt(Eq(Size(Variable(Name "s2")), IntLiteral 2)),
                            NonEmptyStmtSeq(
                                AssertStmt(
                                    IsIn(
                                        Union(Union(SetLit(IntLiteral 1), SetLit(IntLiteral 2)), SetLit(IntLiteral 3)),
                                        Variable(Name "s2")
                                    )
                                ),
                                EmptyStmtSeq
                            )
                        )
                    )
                )
            )
        )
    )

typecheckProg prog9
executeProg prog9

(* success/prog10.txt *)

let prog10 =
    LangProg(
        NonEmptyStmtSeq(
            AssertStmt(
                And(
                    Eq(
                        Union(Union(SetLit(IntLiteral 1), SetLit(IntLiteral 2)), SetLit(IntLiteral 3)),
                        Union(Union(SetLit(IntLiteral 3), SetLit(IntLiteral 2)), SetLit(IntLiteral 1))
                    ),
                    Not(
                        Eq(
                            SetLit(PairLit(PairLit(IntLiteral 1, IntLiteral 2), IntLiteral 3)),
                            SetLit(PairLit(PairLit(IntLiteral 3, IntLiteral 2), IntLiteral 1))
                        )
                    )
                )
            ),
            EmptyStmtSeq
        )
    )

typecheckProg prog10
executeProg prog10

(* success/prog11.txt *)

let prog11 =
    LangProg(
        NonEmptyStmtSeq(
            VarStmt(Name "i", IntLiteral 0),
            NonEmptyStmtSeq(
                WhileStmt(
                    Not(Eq(Variable(Name "i"), IntLiteral 10)),
                    Block(
                        NonEmptyStmtSeq(
                            AssertStmt(
                                Eq(
                                    SetEnum(
                                        Name "i",
                                        Union(
                                            Union(
                                                SetLit(Variable(Name "i")),
                                                SetLit(Add(Variable(Name "i"), IntLiteral 1))
                                            ),
                                            SetLit(Add(Variable(Name "i"), IntLiteral 2))
                                        ),
                                        Mul(IntLiteral 2, Variable(Name "i"))
                                    ),
                                    Union(
                                        Union(
                                            SetLit(Mul(IntLiteral 2, Variable(Name "i"))),
                                            SetLit(Mul(IntLiteral 2, Add(Variable(Name "i"), IntLiteral 1)))
                                        ),
                                        SetLit(Mul(IntLiteral 2, Add(Variable(Name "i"), IntLiteral 2)))
                                    )
                                )
                            ),
                            NonEmptyStmtSeq(AssignStmt(Name "i", Add(Variable(Name "i"), IntLiteral 1)), EmptyStmtSeq)
                        )
                    )
                ),
                EmptyStmtSeq
            )
        )
    )

typecheckProg prog11
executeProg prog11

(* success/prog12.txt *)
let prog12 =
    LangProg(
        NonEmptyStmtSeq(
            VarStmt(
                Name "s1",
                Union(
                    SetLit(Union(Union(SetLit(IntLiteral 1), SetLit(IntLiteral 2)), SetLit(IntLiteral 3))),
                    SetLit(Union(SetLit(IntLiteral 3), SetLit(IntLiteral 4)))
                )
            ),
            NonEmptyStmtSeq(
                VarStmt(
                    Name "s2",
                    SetEnum(
                        Name "x",
                        Variable(Name "s1"),
                        Union(
                            SetEnum(Name "x", Variable(Name "x"), Add(Variable(Name "x"), IntLiteral 1)),
                            SetEnum(Name "x", Variable(Name "x"), Mul(IntLiteral 5, Variable(Name "x")))
                        )
                    )
                ),
                NonEmptyStmtSeq(
                    AssertStmt(
                        And(
                            And(
                                IsIn(
                                    Union(
                                        Union(
                                            Union(
                                                Union(
                                                    Union(SetLit(IntLiteral 2), SetLit(IntLiteral 3)),
                                                    SetLit(IntLiteral 4)
                                                ),
                                                SetLit(IntLiteral 5)
                                            ),
                                            SetLit(IntLiteral 10)
                                        ),
                                        SetLit(IntLiteral 15)
                                    ),
                                    Variable(Name "s2")
                                ),
                                IsIn(
                                    Union(
                                        Union(Union(SetLit(IntLiteral 4), SetLit(IntLiteral 5)), SetLit(IntLiteral 15)),
                                        SetLit(IntLiteral 20)
                                    ),
                                    Variable(Name "s2")
                                )
                            ),
                            Eq(Size(Variable(Name "s2")), IntLiteral 2)
                        )
                    ),
                    EmptyStmtSeq
                )
            )
        )
    )

typecheckProg prog12
executeProg prog12

(* success/prog13.txt *)
let prog13 =
    LangProg(
        NonEmptyStmtSeq(
            VarStmt(
                Name "s1",
                Union(
                    Union(Union(SetLit(IntLiteral 1), SetLit(IntLiteral 2)), SetLit(IntLiteral 3)),
                    SetLit(IntLiteral 4)
                )
            ),
            NonEmptyStmtSeq(
                VarStmt(
                    Name "s2",
                    Union(
                        Union(Union(SetLit(IntLiteral 3), SetLit(IntLiteral 4)), SetLit(IntLiteral 5)),
                        SetLit(IntLiteral 6)
                    )
                ),
                NonEmptyStmtSeq(
                    AssertStmt(
                        And(
                            Eq(
                                Union(Variable(Name "s1"), Variable(Name "s2")),
                                Union(Variable(Name "s2"), Variable(Name "s1"))
                            ),
                            Eq(
                                Union(Variable(Name "s1"), Variable(Name "s2")),
                                Union(
                                    Union(
                                        Union(
                                            Union(
                                                Union(SetLit(IntLiteral 1), SetLit(IntLiteral 2)),
                                                SetLit(IntLiteral 3)
                                            ),
                                            SetLit(IntLiteral 4)
                                        ),
                                        SetLit(IntLiteral 5)
                                    ),
                                    SetLit(IntLiteral 6)
                                )
                            )
                        )
                    ),
                    NonEmptyStmtSeq(
                        AssertStmt(
                            Eq(
                                Variable(Name "s1"),
                                Union(
                                    Union(Union(SetLit(IntLiteral 1), SetLit(IntLiteral 2)), SetLit(IntLiteral 3)),
                                    SetLit(IntLiteral 4)
                                )
                            )
                        ),
                        NonEmptyStmtSeq(
                            AssertStmt(
                                Eq(
                                    Variable(Name "s2"),
                                    Union(
                                        Union(Union(SetLit(IntLiteral 3), SetLit(IntLiteral 4)), SetLit(IntLiteral 5)),
                                        SetLit(IntLiteral 6)
                                    )
                                )
                            ),
                            NonEmptyStmtSeq(
                                AssertStmt(
                                    Eq(
                                        Diff(Variable(Name "s1"), Variable(Name "s2")),
                                        Union(SetLit(IntLiteral 1), SetLit(IntLiteral 2))
                                    )
                                ),
                                NonEmptyStmtSeq(
                                    AssertStmt(
                                        Eq(
                                            Diff(Variable(Name "s2"), Variable(Name "s1")),
                                            Union(SetLit(IntLiteral 5), SetLit(IntLiteral 6))
                                        )
                                    ),
                                    NonEmptyStmtSeq(
                                        AssertStmt(
                                            Eq(
                                                Variable(Name "s1"),
                                                Union(
                                                    Union(
                                                        Union(SetLit(IntLiteral 1), SetLit(IntLiteral 2)),
                                                        SetLit(IntLiteral 3)
                                                    ),
                                                    SetLit(IntLiteral 4)
                                                )
                                            )
                                        ),
                                        NonEmptyStmtSeq(
                                            AssertStmt(
                                                Eq(
                                                    Variable(Name "s2"),
                                                    Union(
                                                        Union(
                                                            Union(SetLit(IntLiteral 3), SetLit(IntLiteral 4)),
                                                            SetLit(IntLiteral 5)
                                                        ),
                                                        SetLit(IntLiteral 6)
                                                    )
                                                )
                                            ),
                                            EmptyStmtSeq
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            )
        )
    )

typecheckProg prog13
executeProg prog13
