window.BpmnInterop = (function () {
    let modeler = null;

    return {
        init: function (containerId) {
            const el = document.getElementById(containerId);
            if (!el) {
                console.error("Container not found");
                return;
            }

            // حتماً height و width واقعی داشته باشد
            if (!el.offsetHeight || !el.offsetWidth) {
                console.warn("Container has no size yet, delaying...");
                setTimeout(() => this.init(containerId), 50);
                return;
            }

            modeler = new BpmnJS({
                container: '#' + containerId,
                keyboard: {bindTo: window}, // فعال‌سازی شورتکات‌ها
            });

            console.log("BPMN Modeler initialized (empty canvas)");
        },

        getModeler: function () {
            return modeler;
        },

        // برای اضافه کردن XML در آینده
        importXML: async function (xml) {
            if (!modeler) {
                console.error("Modeler not initialized");
                return;
            }
            try {
                if (xml.length === 0)
                    xml = `<?xml version="1.0" encoding="UTF-8"?>
                    <bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                        xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL"
                        xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI"
                        xmlns:dc="http://www.omg.org/spec/DD/20100524/DC"
                        xmlns:di="http://www.omg.org/spec/DD/20100524/DI"
                        id="Definitions_1"
                    targetNamespace="http://bpmn.io/schema/bpmn">
                        <bpmn:process id="Process_1" isExecutable="false">
                            <bpmn:startEvent id="StartEvent_1" name="Start" />
                            <bpmn:endEvent id="EndEvent_1" name="End" />
                        </bpmn:process>

                    <bpmndi:BPMNDiagram id="BPMNDiagram_1">
                        <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_1">
                            <bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1">
                                <dc:Bounds x="150" y="100" width="36" height="36" />
                            </bpmndi:BPMNShape>
                            <bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1">
                                <dc:Bounds x="300" y="100" width="36" height="36" />
                            </bpmndi:BPMNShape>
                        </bpmndi:BPMNPlane>
                    </bpmndi:BPMNDiagram>
                </bpmn:definitions>`;
                await modeler.importXML(xml);
                modeler.get('canvas').zoom('fit-viewport');
                console.log("XML imported successfully");
            } catch (err) {
                console.error(err);
            }
        },
        saveState: async function () {
            if (!modeler) {
                console.error("Modeler not initialized");
                return [];
            }

            const elementRegistry = modeler.get('elementRegistry');

            // همه Participants
            const participants = elementRegistry.filter(e => e.type === "bpmn:Participant");

            // فقط المان‌های bpmn (نه labelها و غیره)
            const elements = elementRegistry.filter(e => e.type.startsWith("bpmn:"));

            return elements.map(el => {
                // پیش‌فرض هیچ participantی نداره
                let participantId = null;

                // بررسی کنیم که el داخل کدوم participant هست
                participants.forEach(p => {
                    const flowElements = p.businessObject.processRef.flowElements || [];
                    if (flowElements.some(fe => fe.id === el.businessObject.id)) {
                        participantId = p.id;
                    }
                });

                // برای SequenceFlow
                if (el.type === "bpmn:SequenceFlow") {
                    return {
                        elId: el.id,
                        title: el.businessObject.name || "",
                        type: "SequenceFlow",
                        sourceRef: el.businessObject.sourceRef?.id || null,
                        targetRef: el.businessObject.targetRef?.id || null,
                        participantXmlId: participantId
                    };
                }

                // برای بقیه المنت‌ها
                return {
                    elId: el.id,
                    title: el.businessObject.name || "",
                    type: el.type.replace("bpmn:", ""),
                    participantXmlId: participantId
                };
            });
        },
        exportXML: async function () {
            if (!modeler) {
                console.error("Modeler not initialized");
                return;
            }

            try {
                const {xml} = await modeler.saveXML({format: true}); // خروجی XML
                console.log("Exported XML:", xml);

                return xml; // می‌تونی به C# بفرستی
            } catch (err) {
                console.error("Error exporting XML", err);
                return null;
            }
        },
        registerClickHandler: function (dotNetRef) {
            const eventBus = modeler.get('eventBus');

            eventBus.on('element.click', function (event) {
                dotNetRef.invokeMethodAsync("OnElementClicked", null, null, false,null);
            })
            eventBus.on('element.contextmenu', function (event) {
                event.originalEvent.preventDefault();
                let element = event.element;

                if (element.type === "bpmn:Collaboration") {
                    const selection = modeler.get('selection').get();
                    if (selection && selection.length > 0) {
                        element = selection[0]; // همون فلشی که انتخاب شده
                    } else {
                        return;
                    }
                }
                let sourceElId = null;
                let isGateway = false;

                if (element.type === "bpmn:SequenceFlow") {
                    let source = element.businessObject.sourceRef;
                    const elementRegistry = modeler.get('elementRegistry');

                    if (source.$type.includes("Gateway")) {
                        isGateway = true;
                        const incomingFlows = source.incoming || [];

                        const taskBeforeGateway = incomingFlows
                            .map(f => elementRegistry.get(f.sourceRef.id))
                            .find(e => e.type === "bpmn:Task");

                        if (taskBeforeGateway) {
                            sourceElId = taskBeforeGateway.id;
                        } 
                    } 
                }

                if (element.type.startsWith("bpmn:")) {
                    dotNetRef.invokeMethodAsync("OnElementClicked", element.id, element.type, isGateway, sourceElId);
                } else {
                    dotNetRef.invokeMethodAsync("OnElementClicked", null, null, false, null);
                }
            });
        },
    };
})();
