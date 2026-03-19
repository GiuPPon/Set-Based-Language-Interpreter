package projectLabo.visitors.typechecking;

import static java.util.Objects.requireNonNull;

public record SetType(Type elType) implements Type { 

	public static final String TYPE_NAME = "SET";

	public SetType {
		requireNonNull(elType);
	}

	@Override
	public String toString() {
		// return String.format("{%s}", elType);
		return elType.toString() + " set"; // usiamo questo per matchare la stampa dei test
	}

	@Override
	public SetType toSetType() {
		return this;
	}
}