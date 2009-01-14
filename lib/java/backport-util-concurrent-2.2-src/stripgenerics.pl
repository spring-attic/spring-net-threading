#!/usr/bin/perl

print $#ARGV;
print "AAAA";

if ($#ARGV < 1) { die "usage: stripgenerics.pl <infile> <outfile>"; }

open(IF, $ARGV[0]);
open(OF, ">" . $ARGV[1]);


while ($line = <IF>) {

    $line =~ s/ java\.util\./ edu.emory.mathcs.backport.java.util./g;

    if ($line =~ m/^\s*\* \@param <[A-Za-z,.]+> /) {
        next;
    }
   
    # skip HTML tags
    if ($line =~ m/^\s*\* /) {
        print OF $line;
        next;
    }
   
    $line =~ s/ <[A-Za-z, ]+>//g;
    $line =~ s/<[A-Za-z, ]+>//g;
   
    $line =~ s/<((\? super )?(\? extends )?[A-Za-z .]+,?)+>//g;
    $line =~ s/<((\? super )?(\? extends )?[\?A-Za-z .]+,?)+>//g;

    $line =~ s/ [A-Z] / Object /g;
    $line =~ s/\([A-Z] /(Object /g;
    $line =~ s/\([A-Z]\)//g;
       
    $line =~ s/ [A-Z]\[\]/ Object[]/g;
    $line =~ s/\([A-Z]\[\]/(Object[]/g;
    
    print OF $line;
}

close(OF);
close(IF);
