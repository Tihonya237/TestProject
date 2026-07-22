// Wait for the DOM hierarchy to be fully parsed before binding event handlers
document.addEventListener("DOMContentLoaded", function () {

    // Retrieve active layout elements via native DOM selectors
    const addEmployeeBtn = document.getElementById('add-employee-btn');
    const employeeSearchInput = document.getElementById('employee-search-input');
    const employeesList = document.getElementById('employees-list');
    const selectedEmployeesTableBody = document.querySelector('#selected-employees-table tbody');

    // Ensure all critical components are loaded in the current viewport context
    if (addEmployeeBtn && employeeSearchInput && employeesList && selectedEmployeesTableBody) {

        // Bind a click event listener to the execution trigger button
        addEmployeeBtn.addEventListener('click', function () {
            var inputValue = employeeSearchInput.value.trim();

            // Defensive programming: prevent processing empty inputs
            if (!inputValue) {
                alert('Пожалуйста, начните вводить ФИО сотрудника.');
                return;
            }

            // Query the HTML5 datalist options pool to validate user selection
            var option = employeesList.querySelector('option[value="' + inputValue + '"]');

            if (!option) {
                alert('Пожалуйста, выберите сотрудника из выпадающего списка подсказок.');
                return;
            }

            // Data attributes extraction: fetch database primary keys and reference metadata
            var empId = option.getAttribute('data-id');
            var empEmail = option.getAttribute('data-email') || 'Нет email';

            // Enforce structural duplicate constraints inside the local DOM state
            if (document.getElementById('emp-row-' + empId)) {
                alert('Этот сотрудник уже добавлен в состав команды проекта.');
                return;
            }

            // Construct a symmetric table row layout embedding the parameter mapping array for model binding
            var row = document.createElement('tr');
            row.id = 'emp-row-' + empId;
            row.innerHTML =
                '<td>' +
                inputValue +
                // Name identifier is bound directly to the native C# array signature model binder
                '<input type="hidden" name="selectedEmployeeIds" value="' + empId + '" />' +
                '</td>' +
                '<td>' + empEmail + '</td>' +
                '<td class="text-center">' +
                // Embedded detachment command using close proximity DOM traversing
                '<button type="button" class="btn btn-danger btn-sm" onclick="this.closest(\'tr\').remove();">Удалить</button>' +
                '</td>';

            // Commit layout injection and clear the search query string buffer
            selectedEmployeesTableBody.appendChild(row);
            employeeSearchInput.value = '';
        });
    }
});
