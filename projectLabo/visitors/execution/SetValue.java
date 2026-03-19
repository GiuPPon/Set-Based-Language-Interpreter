package projectLabo.visitors.execution;

import java.util.Collections;
import java.util.HashSet;
import java.util.Set;
import static java.util.Objects.requireNonNull;

public class SetValue implements Value {
    private final Set<Value> elements; // memorizza le informazioni per l'insieme preso in considerazione
    public SetValue(Set<Value> elemento) {
        // l'elemento non deve essere modificato e ne facciamo una copia
        this.elements = Collections.unmodifiableSet(new HashSet<>(requireNonNull(elemento)));
    }

    @Override
    public SetValue toSet() {
      return this; // se SetValue restituisce se stesso
    }
    
    public Set<Value> getSet() {
        return elements;
    }

    // operazioni semantica dinamica
    public int size(){
      return elements.size();
   }
   
   public boolean isIn(Value val){
      return elements.contains(val);
   }

   public SetValue union(SetValue other){
      Set<Value> newSet = new HashSet<>(this.elements);  // copia di A
      newSet.addAll(other.elements);   // B
      return new SetValue(newSet);  // C
   }  

   public SetValue difference(SetValue other){
      Set<Value> newSet = new HashSet<>(this.elements);
      newSet.removeAll(other.elements);
      return new SetValue(newSet);

   }

   @Override
    public String toString() {
        return ("setOfSize(" + elements.size() + ")");
    }

   @Override
   public boolean equals(Object obj) {
      if (this == obj) return true;
      if (obj == null || getClass() != obj.getClass()) 
         return false;
      SetValue other = (SetValue) obj;  // 
      return elements.equals(other.elements);
   }

   @Override
   public int hashCode() {
      return elements.hashCode();
   }

}