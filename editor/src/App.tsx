import { useEffect, useRef } from "react";
import * as Blockly from "blockly";
import "blockly/blocks";

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

        // --- Define custom blocks ---
        Blockly.defineBlocksWithJsonArray([
            {
                "type": "cs_var_decl",
                "message0": "%1 %2 = %3;",
                "args0": [
                    {
                        "type": "input_value",
                        "name": "TYPE"
                    },
                    {
                        "type": "field_variable",
                        "name": "IDENT",
                        "variable": "variable_name"
                    },
                    {
                        "type": "input_value",
                        "name": "VALUE"
                    },
                ],
                "previousStatement": null,
                "nextStatement": null,
                "colour": 160,
            },
            {
                "type": "if_block",
                "message0": "if ( %1 ) {",
                "args0": [
                    {
                        "type": "field_input",
                        "name": "COND",
                        "text": "true"
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
            }
        ]);

        // --- Create workspace ---
        const initialToolbox = {
            "kind": "flyoutToolbox",
            "contents": [
                {
                    "kind": "block",
                    "type": "if_block"
                },
                {
                    "kind": "block",
                    "type": "cs_var_decl"
                }
            ]
        };
        const workspace = Blockly.inject(blocklyDiv.current, {
            toolbox: initialToolbox
        });
        workspaceRef.current = workspace;

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

        return () => workspace.dispose();
    }, []);

    return (
        <div className={'h-screen w-screen flex'}>
            <div className={'h-full w-1/2 bg-red-500'}>

            </div>
            <div
                ref={blocklyDiv}
                className={'h-full w-1/2'}
                style={{ background: "#f4f4f4" }}
            />
        </div>
    );
}
