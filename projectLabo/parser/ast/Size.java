package projectLabo.parser.ast;

import projectLabo.visitors.Visitor;

public class Size extends UnaryOp {
    public Size(Exp exp) { //Salvataggio dei dati
        super(exp); //ci pensa UnaryOp
    }

    @Override
	public <T> T accept(Visitor<T> visitor) {
		return visitor.visitSize(exp);
	}
}