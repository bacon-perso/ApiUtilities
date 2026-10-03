let version = "{version}";
let json = undefined;

document.addEventListener('DOMContentLoaded', () => {
    const observer = new MutationObserver(async (mutationList) => {
        for (const mutation of mutationList) {
            if (Array.from(mutation.removedNodes).some((node) => node.className === "loading-container")) {
                const selectEl = document.querySelector("#select");
                const selectValue = selectEl ? selectEl.value : "";
                let host = `${window.location.protocol}//${window.location.host}${selectValue}`;

                try {
                    const response = await fetch(host);
                    if (response.ok) {
                        json = await response.json();
                    }
                } catch (error) {
                    console.error("Failed to fetch host data:", error);
                }

                version = json?.info?.version;

                const blocks = document.querySelectorAll(".opblock-summary");

                blocks.forEach((block) => {
                    const dataPathEl = block.querySelector("*[data-path]");
                    const path = dataPathEl ? dataPathEl.getAttribute("data-path") : null;

                    const verbEl = block.querySelector(".opblock-summary-method");
                    const verb = verbEl ? verbEl.textContent.trim().toLowerCase() : null;

                    if (path && verb && json?.paths?.[path]?.[verb]) {
                        const accessRights = json.paths[path][verb]["x-access-rights"];
                        const rateLimiting = json.paths[path][verb]["x-ratelimit"];

                        addPrivacyDesc(block, accessRights?.privacy);

                        if (!block.parentElement || !block.parentElement.classList.contains("is-open")) {
                            return;
                        }

                        addPrivileges(block, accessRights?.privileges);
                        addRateLimiting(block, rateLimiting?.["time-in-milliseconds"]);
                    }
                });
            }

            if (mutation.type === "attributes") {
                const target = mutation.target;

                if (mutation.attributeName === "data-is-open" && target.getAttribute("data-is-open") === "true") {
                    const blocks = document.querySelectorAll(".opblock-summary");

                    blocks.forEach((block) => {
                        const dataPathEl = block.querySelector("*[data-path]");
                        const path = dataPathEl ? dataPathEl.getAttribute("data-path") : null;

                        const verbEl = block.querySelector(".opblock-summary-method");
                        const verb = verbEl ? verbEl.textContent.trim().toLowerCase() : null;

                        if (path && verb && json?.paths?.[path]?.[verb]) {
                            addPrivacyDesc(block, json.paths[path][verb]["x-access-rights"]?.privacy);
                        }
                    });
                }

                if (mutation.attributeName === "aria-expanded" && target.getAttribute("aria-expanded") === "true" && target.classList.contains("opblock-summary-control")) {
                    const dataPathEl = target.querySelector("*[data-path]");
                    const path = dataPathEl ? dataPathEl.getAttribute("data-path") : null;

                    const verbEl = target.querySelector(".opblock-summary-method");
                    const verb = verbEl ? verbEl.textContent.trim().toLowerCase() : null;

                    if (path && verb && json?.paths?.[path]?.[verb]) {
                        const accessRights = json.paths[path][verb]["x-access-rights"];
                        const rateLimiting = json.paths[path][verb]["x-ratelimit"];

                        setTimeout(() => {
                            if (target.parentElement) {
                                addPrivileges(target.parentElement, accessRights?.privileges);
                                addRateLimiting(target.parentElement, rateLimiting ? rateLimiting["time-in-milliseconds"] : "");
                            }
                        }, 200);
                    }
                }
            }
        }
    });

    observer.observe(document.getElementById("swagger-ui"), {
        attributes: true,
        childList: true,
        subtree: true,
    });

    document.addEventListener('click', (e) => {
        if (e.target.matches('a.link')) {
            window.location = "/";
        }
    });
});

const addPrivileges = (el, privileges) => {
    const parent = el.parentElement;

    if (!parent) {
        return;
    }

    if (privileges === "Anonymous" || parent.querySelector(".privileges")) {
        return;
    }

    const descWrapper = parent.querySelector(".opblock-section");

    if (descWrapper) {
        const htmlString = `
            <div class="opblock-section privileges">
                <div class="opblock-section-header">
                    <div class="tab-item active">
                        <h4 class="opblock-title">
                            <span>Access Rights</span>
                        </h4>
                    </div>
                </div>
                <div class="parameters-container">
                    <div class="table-container">
                        <table class="parameters">
                            <tbody>
                                <tr>
                                    <td class="parameters-col_name">
                                        <div class="parameter__privileges">Privileges</div>
                                    </td>
                                    <td class="parameters-col_description">
                                        <div class="renderedMarkdown"><p>${privileges}</p></div>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        `;

        descWrapper.insertAdjacentHTML('beforebegin', htmlString);
    }
};

const addRateLimiting = (el, rateLimiting) => {
    const parent = el.parentElement;

    if (!parent) {
        return;
    }

    if (parent.querySelector(".rateLimiting") || !rateLimiting) {
        return;
    }

    const descWrapper = parent.querySelector(".opblock-section");

    if (descWrapper) {
        const htmlString = `
            <div class="opblock-section rateLimiting">
                <div class="opblock-section-header">
                    <div class="tab-item active">
                        <h4 class="opblock-title">
                            <span>Rate limiting</span>
                        </h4>
                    </div>
                </div>
                <div class="parameters-container">
                    <div class="table-container">
                        <table class="parameters">
                            <tbody>
                                <tr>
                                    <td class="parameters-col_name">
                                        <div class="parameter__rateLimiting">Rate limiting</div>
                                    </td>
                                    <td class="parameters-col_description">
                                        <div class="renderedMarkdown"><p>${rateLimiting} ms</p></div>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        `;

        descWrapper.insertAdjacentHTML('beforebegin', htmlString);
    }
};

const addPrivacyDesc = (el, privacyType) => {
    if (el.querySelector(".privacy-stamp")) {
        return;
    }

    const target = el.querySelector(".opblock-summary-description");

    if (target) {
        const htmlString = `
            <small class="privacy-stamp">
                <pre class="privacy ${privacyType
                .toLowerCase()
                .replaceAll(" ", "-")}">${privacyType}</pre>
            </small>
        `;

        target.insertAdjacentHTML('afterend', htmlString);
    }
};