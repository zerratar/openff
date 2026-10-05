// Ghidra headless script: the names match_builds.py found (names.tsv: address, A::B::name, how, the named build's
// address) put on the nameless program's functions, their namespaces made as they go, and the way each was found in
// the function's comment - so a decompile of FF4.exe reads with libff4.so's names.
//
//   analyzeHeadless <project dir> <name> -process -noanalysis -scriptPath Tools/ghidra -preScript ApplyNames.java <names.tsv>

import ghidra.app.script.GhidraScript;
import ghidra.program.model.address.Address;
import ghidra.program.model.listing.Function;
import ghidra.program.model.symbol.Namespace;
import ghidra.program.model.symbol.SourceType;
import ghidra.app.util.NamespaceUtils;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;

public class ApplyNames extends GhidraScript {

	@Override
	protected void run() throws Exception {
		String[] args = getScriptArgs();
		if (args.length < 1) throw new IllegalArgumentException("ApplyNames.java <names.tsv>");
		int named = 0, missing = 0, failed = 0;
		try (BufferedReader r = new BufferedReader(new InputStreamReader(new FileInputStream(args[0]), StandardCharsets.UTF_8))) {
			String line;
			while ((line = r.readLine()) != null) {
				String[] p = line.split("\t");
				if (p.length < 3) continue;
				Address at = toAddr(p[0]);
				Function f = at == null ? null : getFunctionAt(at);
				if (f == null) { missing++; continue; }
				List<String> parts = Split(p[1]);
				try {
					Namespace ns = currentProgram.getGlobalNamespace();
					if (parts.size() > 1) ns = NamespaceUtils.createNamespaceHierarchy(String.join("::", parts.subList(0, parts.size() - 1)), null, currentProgram, SourceType.IMPORTED);
					f.setParentNamespace(ns);
					f.setName(parts.get(parts.size() - 1), SourceType.IMPORTED);
					f.setComment("libff4.so: " + p[1] + (p.length > 3 ? " @ " + p[3] : "") + " - matched by " + p[2]);
					named++;
				}
				catch (Exception ex) { failed++; }
			}
		}
		println("names put on: " + named + ", no function at the address: " + missing + ", refused: " + failed);
	}

	/** A::B<C::D>::name split at the :: outside angle brackets and parentheses. */
	private static List<String> Split(String name) {
		List<String> parts = new ArrayList<>();
		int depth = 0, start = 0;
		for (int i = 0; i < name.length(); i++) {
			char c = name.charAt(i);
			if (c == '<' || c == '(') depth++;
			else if (c == '>' || c == ')') depth--;
			else if (c == ':' && depth == 0 && i + 1 < name.length() && name.charAt(i + 1) == ':') {
				parts.add(name.substring(start, i));
				start = i + 2;
				i++;
			}
		}
		parts.add(name.substring(start));
		return parts;
	}
}
