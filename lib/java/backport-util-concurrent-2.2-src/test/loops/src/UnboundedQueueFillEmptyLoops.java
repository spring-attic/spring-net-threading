/*
 * Written by Doug Lea with assistance from members of JCP JSR-166
 * Expert Group and released to the public domain. Use, modify, and
 * redistribute this code in any way without acknowledgement.
 */

import edu.emory.mathcs.backport.java.util.*;
import edu.emory.mathcs.backport.java.util.concurrent.*;
import edu.emory.mathcs.backport.java.util.concurrent.locks.*;
import edu.emory.mathcs.backport.java.util.concurrent.atomic.*;
import java.util.Random;
import edu.emory.mathcs.backport.java.util.concurrent.helpers.*;

public class UnboundedQueueFillEmptyLoops {
    static int maxSize = 10000;
    static Random rng = new Random(3153122688L);
    static volatile int total;
    static Integer[] numbers;

    public static void main(String[] args) throws Exception {
        Class klass = null;
        if (args.length > 0) {
            try {
                klass = Class.forName(args[0]);
            } catch(ClassNotFoundException e) {
                throw new RuntimeException("Class " + args[0] + " not found.");
            }
        }

        if (args.length > 2)
            maxSize = Integer.parseInt(args[2]);

        System.out.print("Class: " + klass.getName());
        System.out.println(" size: " + maxSize);

        numbers = new Integer[maxSize];
        for (int i = 0; i < maxSize; ++i)
            numbers[i] = new Integer(rng.nextInt(128));

        oneRun(klass, maxSize);
        Thread.sleep(100);
        oneRun(klass, maxSize);
        Thread.sleep(100);
        oneRun(klass, maxSize);

        if (total == 0) System.out.print(" ");
   }

    static void oneRun(Class klass, int n) throws Exception {
        Queue q = (Queue)klass.newInstance();
        int sum = total;
        int m = rng.nextInt(numbers.length);
        long startTime = Utils.nanoTime();
        for (int k = 0; k < n; ++k) {
            for (int i = 0; i < k; ++i) {
                if (m >= numbers.length)
                    m = 0;
                q.offer(numbers[m++]);
            }
            Integer p;
            while ((p = (Integer)q.poll()) != null)
                sum += p.intValue();
        }
        total += sum;
        long endTime = Utils.nanoTime();
        long time = endTime - startTime;
        double secs = (double)(time) / 1000000000.0;
        System.out.println("Time: " + secs);
    }

}
