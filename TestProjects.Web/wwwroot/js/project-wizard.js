// --- WIZARD STEP NAVIGATION ---
// --- WIZARD STEP NAVIGATION WITH INPUT VALIDATION BOUNDARIES ---
function changeStep(currentId, nextId) {
    // Defensive check: Only validate fields when moving forward in the wizard sequence
    if (nextId > currentId) {
        var currentStepContainer = document.getElementById('step-' + currentId);

        // Target all required HTML5 inputs inside the scope of the current isolated container
        var inputs = currentStepContainer.querySelectorAll('input[required], select[required]');
        var isStepValid = true;

        inputs.forEach(function (input) {
            if (!input.checkValidity()) {
                input.reportValidity(); // Highlight standard input field constraints directly to the user
                isStepValid = false;
            }
        });

        // Specific chronology validation rule safeguard for Step 1 dates
        if (currentId === 1) {
            var startDateVal = new Date(document.querySelector('input[name="StartDate"]').value);
            var endDateVal = new Date(document.querySelector('input[name="EndDate"]').value);

            if (endDateVal < startDateVal) {
                alert('Дата окончания проекта не может быть раньше даты начала!');
                isStepValid = false;
            }
        }

        // Prevent state transition if boundary violations exist
        if (!isStepValid)
            return; 
    }

    var currentStep = document.getElementById('step-' + currentId);
    if (currentStep)
        currentStep.classList.add('d-none');

    var nextStep = document.getElementById('step-' + nextId);
    if (nextStep)
        nextStep.classList.remove('d-none');
}


// --- PROJECT MANAGER SELECTION HANDLER (Step 3) ---
const managerSearchInput = document.getElementById('manager-search-input');
const managersList = document.getElementById('managers-list');
const hiddenManagerId = document.getElementById('hidden-manager-id');

if (managerSearchInput && managersList && hiddenManagerId) {
    // Intercept the input event to map the selected FullName to its respective database primary key (ID)
    managerSearchInput.addEventListener('input', function () {
        var inputValue = managerSearchInput.value.trim();
        var option = managersList.querySelector('option[value="' + inputValue + '"]');

        if (option) {
            // Match found: populate the hidden input field for backend model binding
            hiddenManagerId.value = option.getAttribute('data-id');
        } else {
            // Reset the internal ID state if the input is cleared or corrupted by manual typing
            hiddenManagerId.value = '';
        }
    });
}

// Initialize complex UI event handlers strictly after the DOM hierarchy is fully parsed
document.addEventListener("DOMContentLoaded", function () {

    // --- PROJECT PARTICIPANTS DYNAMIC MANAGEMENT (Step 4) ---
    const addEmployeeBtn = document.getElementById('add-employee-btn');
    const employeeSearchInput = document.getElementById('employee-search-input');
    const employeesList = document.getElementById('employees-list');
    const selectedEmployeesTableBody = document.querySelector('#selected-employees-table tbody');

    if (addEmployeeBtn && employeeSearchInput && employeesList && selectedEmployeesTableBody) {

        addEmployeeBtn.addEventListener('click', function () {
            var inputValue = employeeSearchInput.value.trim();

            // Validate user input against available options inside the datalist pool
            var option = employeesList.querySelector('option[value="' + inputValue + '"]');

            if (!option) {
                alert('Пожалуйста, выберите сотрудника из выпадающего списка подсказок.');
                return;
            }

            var empId = option.getAttribute('data-id');
            var empEmail = option.getAttribute('data-email');

            // Enforce duplicate restriction checks inside the local DOM state
            if (document.getElementById('emp-row-' + empId)) {
                alert('Этот сотрудник уже добавлен.');
                return;
            }

            // Append a new row layout embedding hidden inputs corresponding to the backend DTO collection name
            var row = document.createElement('tr');
            row.id = 'emp-row-' + empId;
            row.innerHTML =
                '<td>' + inputValue + '<input type="hidden" name="SelectedEmployeeIds" value="' + empId + '" /></td>' +
                '<td>' + empEmail + '</td>' +
                '<td class="text-center"><button type="button" class="btn btn-danger btn-sm" onclick="this.closest(\'tr\').remove();">Удалить</button></td>';

            // Commit row insertion and reset the autocomplete query state
            selectedEmployeesTableBody.appendChild(row);
            employeeSearchInput.value = '';
        });
    }

    // --- ASYNCHRONOUS DRAG & DROP FILE MANAGEMENT (Step 5) ---
    const dropZone = document.getElementById('drop-zone');
    const fileInput = document.getElementById('file-input');
    const fileList = document.getElementById('file-list');
    const mockText = document.getElementById('empty-files-mock');

    if (dropZone) {
        // Delegate drop-zone element clicks directly into the hidden file input handler
        dropZone.addEventListener('click', () => fileInput.click());

        // Override and suppress generic native browser drag/drop behaviors globally
        window.addEventListener("dragover", function (e) { e.preventDefault(); }, false);
        window.addEventListener("drop", function (e) { e.preventDefault(); }, false);

        // Highlight drop target area layout when shifting file buffers
        ['dragenter', 'dragover'].forEach(name => {
            dropZone.addEventListener(name, (e) => {
                e.preventDefault();
                e.stopPropagation();
                dropZone.classList.add('bg-secondary', 'text-white');
            }, false);
        });

        ['dragleave', 'drop'].forEach(name => {
            dropZone.addEventListener(name, (e) => { e.preventDefault();e.stopPropagation();
                dropZone.classList.remove('bg-secondary', 'text-white');
            }, false);
        });

        // Intercept dropped file buffers and bind them natively to the form payload context
        dropZone.addEventListener('drop', (e) => {e.preventDefault();e.stopPropagation();

            if (e.dataTransfer.files.length > 0)
            {
                fileInput.files = e.dataTransfer.files; // Inject the file transfer payload data
                showFiles(e.dataTransfer.files);
            }
        }, false);

        // Native file browser change callback mapping
        fileInput.addEventListener('change', (e) => {
            showFiles(e.target.files);
        });
    }

    // Dynamic UI list generator tracker for staging local uploaded binary payloads
    function showFiles(files) {
        if (!fileList) return;
        fileList.innerHTML = '';
        if (files.length === 0) {
            if (mockText) fileList.appendChild(mockText);
            return;
        }
        for (let file of files) {
            let li = document.createElement('li');
            li.className = 'list-group-item d-flex justify-content-between align-items-center bg-white text-dark';
            li.textContent = file.name + " (" + (file.size / 1024).toFixed(1) + " KB)";
            fileList.appendChild(li);
        }
    }
});
