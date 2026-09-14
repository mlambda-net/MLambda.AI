// hilbert.js — colour for Hilbert code blocks, which no highlighter knows.
//
// A PLAIN FENCE IS READ, NOT ASSUMED. Only a block that looks like Hilbert (a keyword such as
// `theorem`, `law` or `agent`, or one of ∀ ⇒ ↦ ≔) is coloured, and it is labelled with the dialect it
// most resembles: `.hp` proofs, `.ha` agents, `.hs` theories, `.hb` mathematics. Console output is
// left as it is, except that a verdict — Proved, Rejected — is picked out wherever it appears.
(function () {
  "use strict";

  var KEYWORDS = [
    "theory", "def", "law", "query", "modality", "sort", "open", "axioms", "theorem", "lemma", "axiom",
    "fn", "model", "process", "weight", "train", "derive", "field", "draw", "bind",
    "agent", "belief", "desire", "intend", "commit", "attention", "plan", "achieved", "unreachable",
    "because", "when", "then", "if", "else", "where", "from", "extends", "in", "return"
  ];
  var TACTICS = ["proof", "qed", "intro", "apply", "have", "by", "ring", "linarith", "auto", "exact", "sorry"];

  // A DECLARATION STARTS A LINE. Prose in console output ("one agent seeing ruin") must not count.
  var LOOKS_LIKE_HILBERT = /^\s*(theory|theorem|law|agent|plan|query|proof|qed|axioms|open|modality|def|fn|model|belief|desire|intend|attention|commit)\b|[∀⇒↦≔]/m;

  var TOKEN = new RegExp(
    [
      "(--[^\\n]*)",                                        // 1 comment
      "(\"[^\"\\n]*\")",                                    // 2 string
      "\\b(Proved|Assumed)\\b",                             // 3 good verdict
      "\\b(Rejected|Admitted|refused)\\b",                  // 4 bad verdict
      "\\b(" + TACTICS.join("|") + ")\\b",                  // 5 tactic
      "\\b(" + KEYWORDS.join("|") + ")\\b",                 // 6 keyword
      "(\\b\\d+(?:\\.\\d+)?\\b)",                           // 7 number
      "([∀∃⇒∧∨¬↦≔≤≥≠·−⊤⊥□◇→×∂∑]|:-)"                        // 8 symbol
    ].join("|"),
    "g"
  );

  var VERDICT = /\b(Proved|Assumed|Rejected|Admitted)\b/g;

  function escape(text) {
    return text.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
  }

  function dialect(text) {
    if (/\b(theorem|proof|qed|axioms)\b/.test(text)) return ".hp · proof";
    if (/\b(agent|belief|desire|intend|plan|attention|commit)\b/.test(text)) return ".ha · agent";
    if (/\b(theory|query|modality)\b|:-/.test(text)) return ".hs · theory";
    if (/\b(fn|model|process|weight|train)\b|[↦≔]/.test(text)) return ".hb · mathematics";
    return "hilbert";
  }

  function colour(text) {
    var out = "";
    var last = 0;
    var match;
    TOKEN.lastIndex = 0;
    while ((match = TOKEN.exec(text)) !== null) {
      out += escape(text.slice(last, match.index));
      var cls = match[1] ? "ml-com"
        : match[2] ? "ml-str"
        : match[3] ? "ml-ok"
        : match[4] ? "ml-bad"
        : match[5] ? "ml-tac"
        : match[6] ? "ml-kw"
        : match[7] ? "ml-num"
        : "ml-sym";
      out += '<span class="' + cls + '">' + escape(match[0]) + "</span>";
      last = match.index + match[0].length;
    }
    return out + escape(text.slice(last));
  }

  function verdicts(text) {
    return escape(text).replace(VERDICT, function (word) {
      return '<span class="' + (word === "Proved" || word === "Assumed" ? "ml-ok" : "ml-bad") + '">' + word + "</span>";
    });
  }

  function paint(root) {
    var blocks = root.querySelectorAll(".highlight pre > code");
    Array.prototype.forEach.call(blocks, function (code) {
      var box = code.closest(".highlight");
      if (!box || box.dataset.mlPainted) return;
      // A fence with a language (csharp, bash, xml) is already coloured by Pygments.
      if (/\blanguage-(?!text\b)/.test(box.className) || code.querySelector("span[class]:not(:empty)")) {
        box.dataset.mlPainted = "skip";
        return;
      }
      var text = code.textContent;
      if (LOOKS_LIKE_HILBERT.test(text)) {
        code.innerHTML = colour(text);
        box.classList.add("ml-hilbert");
        box.setAttribute("data-dialect", dialect(text));
      } else if (/\b(Proved|Assumed|Rejected|Admitted)\b/.test(text)) {
        code.innerHTML = verdicts(text);
      }
      box.dataset.mlPainted = "yes";
    });
  }

  if (window.document$ && typeof window.document$.subscribe === "function") {
    window.document$.subscribe(function () { paint(document); });
  } else {
    document.addEventListener("DOMContentLoaded", function () { paint(document); });
  }
})();
