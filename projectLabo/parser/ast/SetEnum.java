package projectLabo.parser.ast;

import projectLabo.visitors.Visitor;
import static java.util.Objects.requireNonNull;

public class SetEnum implements Exp {
    private final Variable var;
    private final Exp setExp;
    private final Exp elemExp;

    public SetEnum(Variable var, Exp setExp, Exp elemExp) {
		this.var=requireNonNull(var);
		this.setExp=requireNonNull(setExp);
		this.elemExp=requireNonNull(elemExp);
	}

    @Override
	public <T> T accept(Visitor<T> visitor) {
		return visitor.visitSetEnum(var, setExp, elemExp);
	}
}