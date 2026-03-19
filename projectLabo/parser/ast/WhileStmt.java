package projectLabo.parser.ast;

import projectLabo.visitors.Visitor;
import static java.util.Objects.requireNonNull;

public class WhileStmt implements Stmt {

    private final Exp exp;
    private final Block block;

    public WhileStmt(Exp exp, Block block) {
        this.exp = requireNonNull(exp);
        this.block = requireNonNull(block);
    }

    @Override
    public <T> T accept(Visitor<T> visitor) {
        return visitor.visitWhileStmt(exp, block);
    }
}
