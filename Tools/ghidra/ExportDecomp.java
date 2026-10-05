// Ghidra headless post-script: every function of the program decompiled to C, and the facts the
// cross-build matcher (Tools/ghidra/match_builds.py) reads - what each function is called, how big it
// is, which strings it refers to and which functions it calls.
//
//   analyzeHeadless <project dir> <name> -import <binary> -scriptPath Tools/ghidra -postScript ExportDecomp.java <out dir>
//
// <out dir>/decomp/<namespace>.c  the decompiled functions, grouped by their top-level C++ namespace
//                                 (by address range, 400 a file, where the binary has no names: FF4.exe)
// <out dir>/facts.tsv             address, name, size, strings (\u001f between), callees (addresses, comma)
//
// What comes out is the game's code: it stays out of the repository (.gitignore: Reference/libff4/decomp,
// Reference/ff4exe).

import ghidra.app.decompiler.DecompInterface;
import ghidra.app.decompiler.DecompileResults;
import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.data.StringDataInstance;
import ghidra.program.model.listing.*;
import ghidra.program.model.symbol.*;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.*;

public class ExportDecomp extends GhidraScript {

	@Override
	protected void run() throws Exception {
		String[] args = getScriptArgs();
		File out = new File(args.length > 0 ? args[0] : "ghidra-out");
		File decompDir = new File(out, "decomp");
		decompDir.mkdirs();

		DecompInterface decomp = new DecompInterface();
		decomp.toggleCCode(true);
		decomp.toggleSyntaxTree(false);
		decomp.setSimplificationStyle("decompile");
		if (!decomp.openProgram(currentProgram)) throw new IllegalStateException("decompiler: " + decomp.getLastMessage());

		Map<String, Writer> files = new HashMap<>();
		Listing listing = currentProgram.getListing();
		ReferenceManager refs = currentProgram.getReferenceManager();
		int count = 0, failed = 0;
		try (Writer facts = new BufferedWriter(new OutputStreamWriter(new FileOutputStream(new File(out, "facts.tsv")), StandardCharsets.UTF_8))) {
			FunctionIterator it = listing.getFunctions(true);
			while (it.hasNext() && !monitor.isCancelled()) {
				Function f = it.next();
				if (f.isThunk() || f.isExternal()) continue;

				// The facts: strings referred to from the body, and the functions called.
				Set<String> strings = new LinkedHashSet<>();
				Set<String> callees = new LinkedHashSet<>();
				for (Instruction ins : listing.getInstructions(f.getBody(), true)) {
					for (Reference r : refs.getReferencesFrom(ins.getAddress())) {
						Address to = r.getToAddress();
						if (r.getReferenceType().isCall()) {
							Function c = getFunctionAt(to);
							if (c != null && c.isThunk()) c = c.getThunkedFunction(true);
							if (c != null) callees.add(c.getEntryPoint().toString());
						}
						else if (r.getReferenceType().isData()) {
							Data d = listing.getDataAt(to);
							if (d != null && d.hasStringValue()) {
								String s = StringDataInstance.getStringDataInstance(d).getStringValue();
								if (s != null && s.length() >= 4) strings.add(s.replace('\t', ' ').replace('\n', ' ').replace('\r', ' ').replace('\u001f', ' '));
							}
						}
					}
				}
				facts.write(f.getEntryPoint() + "\t" + f.getName(true) + "\t" + f.getBody().getNumAddresses() + "\t"
					+ String.join("\u001f", strings) + "\t" + String.join(",", callees) + "\n");

				// The C, in the file of its namespace (or of its address range).
				String group = Group(f, count);
				Writer w = files.get(group);
				if (w == null) {
					w = new BufferedWriter(new OutputStreamWriter(new FileOutputStream(new File(decompDir, group + ".c")), StandardCharsets.UTF_8));
					files.put(group, w);
				}
				DecompileResults res = decomp.decompileFunction(f, 60, monitor);
				w.write("// == " + f.getName(true) + " @ " + f.getEntryPoint() + " (" + f.getBody().getNumAddresses() + " bytes)\n");
				if (res != null && res.decompileCompleted() && res.getDecompiledFunction() != null) w.write(res.getDecompiledFunction().getC());
				else { w.write("// decompile failed: " + (res == null ? "?" : res.getErrorMessage()) + "\n"); failed++; }
				w.write("\n");
				if (++count % 500 == 0) println("decompiled " + count + " functions");
			}
		}
		finally {
			for (Writer w : files.values()) w.close();
			decomp.dispose();
		}
		println("done: " + count + " functions, " + failed + " not decompiled, " + files.size() + " files in " + decompDir);
	}

	/** The function's top-level namespace (FUN_ ones of a nameless binary by address, 400 a file). */
	private static String Group(Function f, int index) {
		Namespace ns = f.getParentNamespace();
		Namespace top = null;
		while (ns != null && !ns.isGlobal()) { top = ns; ns = ns.getParentNamespace(); }
		if (top != null) return Clean(top.getName());
		if (f.getName().startsWith("FUN_")) return "unnamed_" + String.format("%03d", index / 400);
		return "_global";
	}

	private static String Clean(String s) {
		return s.replaceAll("[^A-Za-z0-9_.-]", "_");
	}
}
