/*
 * Written by Josh Bloch and Doug Lea with assistance from members of
 * JCP JSR-166 Expert Group and released to the public domain, as
 * explained at http://creativecommons.org/licenses/publicdomain
 */
/*
 * @test
 * @synopsis Checks that a priority queue returns elements in sorted order across various operations
 */

import edu.emory.mathcs.backport.java.util.PriorityQueue;
import java.util.Comparator;
import java.util.ArrayList;
import java.util.List;
import java.util.Iterator;
import edu.emory.mathcs.backport.java.util.Collections;
import edu.emory.mathcs.backport.java.util.Queue;

public class PriorityQueueSort {

    static class MyComparator implements Comparator {
        public int compare(Object x, Object y) {
            int i = ((Integer)x).intValue();
            int j = ((Integer)y).intValue();
            if (i < j) return -1;
            if (i > j) return 1;
            return 0;
        }
    }

    public static void main(String[] args) {
        int n = 100000;
        if (args.length > 0)
            n = Integer.parseInt(args[0]);

        List sorted = new ArrayList(n);
        for (int i = 0; i < n; i++)
            sorted.add(new Integer(i));
        List shuffled = new ArrayList(sorted);
        Collections.shuffle(shuffled);

        Queue pq = new PriorityQueue(n, new MyComparator());
        for (Iterator i = shuffled.iterator(); i.hasNext(); )
            pq.add(i.next());

        List recons = new ArrayList();
        while (!pq.isEmpty())
            recons.add(pq.remove());
        if (!recons.equals(sorted))
            throw new RuntimeException("Sort test failed");

        recons.clear();
        pq = new PriorityQueue(shuffled);
        while (!pq.isEmpty())
            recons.add(pq.remove());
        if (!recons.equals(sorted))
            throw new RuntimeException("Sort test failed");

        // Remove all odd elements from queue
        pq = new PriorityQueue(shuffled);
        for (Iterator i = pq.iterator(); i.hasNext(); )
            if ((((Integer)i.next()).intValue() & 1) == 1)
                i.remove();
        recons.clear();
        while (!pq.isEmpty())
            recons.add(pq.remove());

        for (Iterator i = sorted.iterator(); i.hasNext(); )
            if ((((Integer)i.next()).intValue() & 1) == 1)
                i.remove();

        if (!recons.equals(sorted))
            throw new RuntimeException("Iterator remove test failed.");
    }
}

