import { useEffect, useRef, useState } from "react";
import * as Blockly from "blockly";
import "blockly/blocks";
import { Editor } from "@monaco-editor/react";

export default function App() {
    const blocklyDiv = useRef<HTMLDivElement | null>(null);
    const workspaceRef = useRef<Blockly.WorkspaceSvg | null>(null);

    // Keep the *base* toolbox JSON in a ref so we can clone it later
    const baseToolboxRef = useRef<any>({
        kind: "categoryToolbox",
        contents: [
            {
                kind: "category",
                name: "Statements",
                contents: [
                    { kind: "block", type: "if_block" },
                    { kind: "block", type: "class_block" },
                ],
            },
            {
                kind: "category",
                name: "Variables",
                contents: [
                    // { kind: "block", type: "var_reference" },
                    { kind: "button", text: "Add Variable", callbackKey: "addVariable" },
                ],
            },
            {
                kind: "category",
                name: "Functions",
                contents: [
                    // { kind: "block", type: "func_reference" },
                    { kind: "block", type: "method_block" },
                ],
            },
        ],
    });

    const [blocklyWorkspaceState, setblocklyWorkspaceState] = useState<any>(null);
    const editorRef = useRef<any>(null);

    // --- Inject workspace once on mount ---
    useEffect(() => {
        if (!blocklyDiv.current) return;
        // define static blocks (unchanged)
        Blockly.defineBlocksWithJsonArray([
            /* your static blocks: if_block, class_block, method_block, var_reference, func_reference ... */
            {
                "type": "if_block",
                "message0": "if (%1) \n{",
                "args0": [
                    {
                        "type": "input_value",
                        "name": "COND",
                    }
                ],
                "message1": "%1",
                "args1": [
                    {
                        "type": "input_statement",
                        "name": "BODY"
                    }
                ],
                "message2": "}",
                "colour": 210,
                "previousStatement": null,
                "nextStatement": null,
                "inputsInline": true
            },
            {
                "type": "class_block",
                "message0": "class %1 \n{",
                "args0": [
                    {
                        "type": "field_input",
                        "name": "IDENT",
                    }
                ],
                "message1": "%1",
                "args1": [
                    {
                        "type": "input_statement",
                        "name": "BODY"
                    }
                ],
                "message2": "}",
                "colour": 210,
                "previousStatement": null,
                "nextStatement": null,
                "inputsInline": true
            },
            {
                type: "method_block",
                message0: "%1 %2(%3) \n{\n%4\n}",
                args0: [
                    {
                        type: "field_input",
                        name: "MODIFIERS",
                    },
                    {
                        type: "field_input",
                        name: "IDENT",
                    },
                    {
                        type: "field_input",
                        name: "PARAMS",
                        text: ""
                    },
                    {
                        type: "input_statement",
                        name: "BODY"
                    }
                ],
                colour: 230,
                previousStatement: null,
                nextStatement: null,
                inputsInline: true
            },
            // {
            //     type: "var_reference",
            //     message0: "%1",
            //     args0: [
            //         {
            //             type: "field_dropdown",
            //             name: "VAR",
            //             options: () => {
            //                 return [["a"], ["a"]]
            //                 // const options = Array.from(variables).map((v) => [v, v]);
            //                 // return options.length > 0 ? options : [["", ""]]
            //             }
            //         }
            //     ],
            //     output: null,
            //     colour: 230
            // },
            // {
            //     type: "func_reference",
            //     message0: "%1()",
            //     args0: [
            //         {
            //             type: "field_dropdown",
            //             name: "VAR",
            //             options: () => {
            //                 return [["a"], ["a"]]
            //                 // const options = Array.from(functions).map((v) => [v, v]);
            //                 // return options.length > 0 ? options : [["", ""]]
            //             }
            //         }
            //     ],
            //     output: null,
            //     colour: 230
            // },
        ]);

        const workspace = Blockly.inject(blocklyDiv.current, {
            toolbox: JSON.parse(JSON.stringify(baseToolboxRef.current)),
        });

        workspaceRef.current = workspace;
        workspace.getFlyout().autoClose = false;

        // register button callback (works on the stable workspace)
        workspace.registerButtonCallback("addVariable", () => {
            const variableType = prompt("Enter variable type (e.g., int, string):");
            const variableName = prompt("Enter variable name:");
            if (!variableType || !variableName) return;

            const blockType = `var_decl_${variableName}`;
            Blockly.defineBlocksWithJsonArray([
                {
                    type: blockType,
                    message0: "%1 %2 = %3;",
                    args0: [
                        { type: "field_label_serializable", name: "TYPE", text: variableType },
                        { type: "field_label_serializable", name: "IDENT", text: variableName },
                        { type: "input_value", name: "VALUE" },
                    ],
                    previousStatement: null,
                    nextStatement: null,
                    colour: 160,
                },
            ]);

            // clone toolbox, mutate clone, then update
            const toolboxClone = JSON.parse(JSON.stringify(baseToolboxRef.current));
            const variableCategory = toolboxClone.contents.find(
                (c: any) => c.kind === "category" && c.name === "Variables"
            );
            if (variableCategory && "contents" in variableCategory) {
                variableCategory.contents.push({ kind: "block", type: blockType });
            }
            workspace.updateToolbox(toolboxClone);
        });

        // cleanup on unmount
        return () => workspace.dispose();
    }, []); // empty deps -> run once

    // --- When backend state (or transpile result) arrives, define dynamic blocks & update toolbox ---
    useEffect(() => {
        console.log(JSON.stringify(blocklyWorkspaceState))
        const workspace = workspaceRef.current;
        if (!workspace || !blocklyWorkspaceState) return;

        // define all dynamic blocks first
        for (const f of Object.values(blocklyWorkspaceState.functions || {})) {
            // f must be a valid block JSON object
            Blockly.defineBlocksWithJsonArray([f]);
        }

        // clone base toolbox, add dynamic block entries into the desired category
        const toolboxClone = JSON.parse(JSON.stringify(baseToolboxRef.current));
        const functionsCategory = toolboxClone.contents.find(
            (c: any) => c.kind === "category" && c.name === "Functions"
        );

        if (functionsCategory && "contents" in functionsCategory) {
            for (const f of Object.values(blocklyWorkspaceState.functions || {})) {
                functionsCategory.contents.push({ kind: "block", type: f.type });
            }
        }

        // CRUCIAL: pass a brand-new object so Blockly rebuilds the flyout
        workspace.updateToolbox(toolboxClone);

        // Blockly.Blocks = { ...Blockly.Blocks }
        workspace.clear()

        // Blockly.serialization.workspaces.load(blocklyWorkspaceState.blocks, workspace)

        // optional: do not clear workspace unless you truly want to
        // workspace.clear();
    }, [blocklyWorkspaceState]);

    return (
        <div className={"h-screen w-screen flex"}>
            <div
                className={"absolute z-[99999] left-2/5 top-5 bg-blue-400 p-2 cursor-pointer"}
                onClick={async () => {
                    console.log(Blockly.serialization.workspaces.save(workspaceRef))

                    const response = await fetch("http://localhost:5020/code-to-blocks", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({ code: editorRef.current.getValue() }),
                    });
                    const data = await response.json();
                    console.log(data)
                    // setblocklyWorkspaceState(data);
                    setblocklyWorkspaceState(`{"blocks":[{"type":"if_block","id":"block1","x":20,"y":20,"inputs":{"COND":{"block":{"type":"math_number","id":"cond1","fields":{"NUM":0}}},"BODY":{"blocks":[]}},"next":null,"previous":null,"collapsed":false,"disabled":false,"inline":true}]}`)
                }}
            >
                Transpile
            </div>

            <div className={"h-full w-1/2 bg-red-500"}>
                <Editor
                    onMount={(editor) => (editorRef.current = editor)}
                    defaultLanguage="csharp"
                    defaultValue={`class Program
{
    static void Main()
    {
      System.Console.WriteLine("Hello World!");    
    }
}`}
                />
            </div>
            <div ref={blocklyDiv} className={"h-full w-1/2"} />
        </div>
    );
}
