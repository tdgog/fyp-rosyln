import { useEffect, useRef } from "react";
import * as Blockly from "blockly";
import "blockly/blocks";
import {Editor} from "@monaco-editor/react";

function addRandomBlock() {
    const id = `dynamic_block_${Math.floor(Math.random() * 10000)}`;

    Blockly.defineBlocksWithJsonArray([
        {
            type: id,
            message0: `${id}`,
            previousStatement: null,
            nextStatement: null,
            colour: 300,
        }
    ]);

    return id;
}

export default function App() {
    const blocklyDiv = useRef<HTMLDivElement>(null);
    const workspaceRef = useRef<Blockly.WorkspaceSvg | null>(null);

    useEffect(() => {
        if (!blocklyDiv.current) return;

        const variables = new Set<string>();
        const functions = new Set<string>();

        // --- Define custom blocks ---
        Blockly.defineBlocksWithJsonArray([
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
                type: "var_reference",
                message0: "%1",
                args0: [
                    {
                        type: "field_dropdown",
                        name: "VAR",
                        options: () => {
                            const options = Array.from(variables).map((v) => [v, v]);
                            return options.length > 0 ? options : [["", ""]]
                        }
                    }
                ],
                output: null,
                colour: 230
            },
            {
                type: "func_reference",
                message0: "%1()",
                args0: [
                    {
                        type: "field_dropdown",
                        name: "VAR",
                        options: () => {
                            const options = Array.from(functions).map((v) => [v, v]);
                            return options.length > 0 ? options : [["", ""]]
                        }
                    }
                ],
                output: null,
                colour: 230
            },
            {"type":"func_decl_System.Console.WriteLine","message0":"System.Console.WriteLine(%1);","args0":[{"type":"field_input","name":"value","text":null}],"colour":30,"previousStatement":null,"nextStatement":null,"inputsInline":true}
        ]);

        // --- Create workspace ---
        const initialToolbox = {
            "kind": "categoryToolbox",
            "contents": [
                {
                    "kind": "category",
                    "name": "Statements",
                    "contents": [
                        {
                            "kind": "block",
                            "type": "if_block"
                        },
                        {
                            "kind": "block",
                            "type": "class_block"
                        },
                        {
                            "kind": "block",
                            "type": "func_decl_System.Console.WriteLine"
                        }
                    ]
                },
                {
                    "kind": "category",
                    "name": "Variables",
                    "contents": [
                        {
                            "kind": "block",
                            "type": "var_reference"
                        },
                        {
                            "kind": "button",
                            "text": "Add Variable",
                            "callbackKey": "addVariable"
                        }
                    ]
                },
                {
                    "kind": "category",
                    "name": "Functions",
                    "contents": [
                        {
                            "kind": "block",
                            "type": "func_reference"
                        },
                        {
                            "kind": "button",
                            "text": "Add Function",
                            "callbackKey": "addFunction"
                        }
                    ]
                },
            ]
        };
        const workspace = Blockly.inject(blocklyDiv.current, {
            toolbox: initialToolbox
        });
        workspaceRef.current = workspace;
        workspace.getFlyout().autoClose = false

        const onKey = (e: KeyboardEvent) => {
            if (e.key.toLowerCase() === "a") {
                initialToolbox.contents.push({
                    kind: "block",
                    type: addRandomBlock()
                });
                workspace.updateToolbox(initialToolbox);
            }
        };
        window.addEventListener("keydown", onKey);

        workspace.registerButtonCallback("addVariable", () => {
            const variableType = prompt("Enter variable type (e.g., int, string):");
            const variableName = prompt("Enter variable name:");

            if (!variableType || !variableName) return;

            const blockType = `var_decl_${variableName}`;
            variables.add(variableName)

            Blockly.defineBlocksWithJsonArray([
                {
                    type: blockType,
                    message0: "%1 %2 = %3;",
                    args0: [
                        {
                            type: "field_label_serializable",
                            name: "TYPE",
                            text: variableType
                        },
                        {
                            type: "field_label_serializable",
                            name: "IDENT",
                            text: variableName
                        },
                        {
                            type: "input_value",
                            name: "VALUE"
                        }
                    ],
                    previousStatement: null,
                    nextStatement: null,
                    colour: 160
                }
            ]);

            const variableCategory = initialToolbox.contents.find(
                (category) => category.kind === "category" && category.name === "Variables"
            );

            if (variableCategory && "contents" in variableCategory) {
                variableCategory.contents.push({
                    kind: "block",
                    type: blockType
                });
            }

            workspace.updateToolbox(initialToolbox);
        });

        workspace.registerButtonCallback("addFunction", () => {
            const functionName = prompt("Enter function name:");

            if (!functionName) return;

            const blockType = `func_decl_${functionName}`;
            functions.add(functionName)

            Blockly.defineBlocksWithJsonArray([
                {
                    type: blockType,
                    message0: "%1 %2(%3) \n{\n%4\n}",
                    args0: [
                        {
                            type: "field_input",
                            name: "MODIFIERS",
                        },
                        {
                            type: "field_input",
                            name: "FUNC_NAME",
                            text: functionName
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
            ]);

            const variableCategory = initialToolbox.contents.find(
                (category) => category.kind === "category" && category.name === "Functions"
            );

            if (variableCategory && "contents" in variableCategory) {
                variableCategory.contents.push({
                    kind: "block",
                    type: blockType
                });
            }

            workspace.updateToolbox(initialToolbox);
        });

        return () => workspace.dispose();
    }, []);

    return (
        <div className={'h-screen w-screen flex'}>
            <div className={'h-full w-1/2 bg-red-500'}>
                <Editor
                    defaultLanguage="csharp"
                    defaultValue={"class Program\n" +
                        "{\n" +
                        "    static void Main()\n" +
                        "    {\n" +
                        "      Console.WriteLine(\"Hello World!\");    \n" +
                        "    }\n" +
                        "}\n"}
                />
            </div>
            <div
                ref={blocklyDiv}
                className={'h-full w-1/2'}
                style={{ background: "#f4f4f4" }}
            />
        </div>
    );
}
