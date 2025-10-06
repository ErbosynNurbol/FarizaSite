(function () {
    var QSurveys = function () {}
    QSurveys.prototype = {
        constructor: QSurveys,
        $surveyForm: $('form.survey-form'),
        addQuestion: function (e) {
            var ev = e || window.event,
                questionWarpper = this.$surveyForm.find('div.question-warpper'),
                currentQuestions = questionWarpper.find('div.question-content'),
                questionIndex = currentQuestions.length + 1,
                questionsTemplate = currentQuestions.eq(0).clone();
            questionsTemplate.removeAttr("data-questionid");
            questionsTemplate.attr("data-isremove", "false");
            questionsTemplate.find("small.surveys-question-index").text(questionIndex);
            questionsTemplate.find('textarea.survey-question-title').val("");
            questionsTemplate.find('input[type="radio"].survey-question-type,input[type="checkbox"].survey-question-type').each(function (i, v) {
                var name = "survey-question-type" + questionIndex,
                    id = "survey-question-type" + questionIndex + (i + 1);
                $(v).attr("name", name).attr("id", id);
                $(v).next('label').attr("for", id);
                $(v).prop("checked", $(v).val() == 1);
            });
            questionsTemplate.find("div.question-option").each(function (i, v) {
                if (i < 2) {
                    $(v).removeClass("d-none").attr("data-isremove", "false").removeAttr("data-optionid");
                    $(v).find(".question-option-value").val("");
                    $(v).find('button.remove-option-btn').addClass('d-none');
                    var c = $(v).find('div.option-correct-answer input.form-check-input');
                    c.prop('checked', false);
                    c.attr('id', 'correctAnswer' + questionIndex + (i + 1));
                    c.attr('name', 'correctAnswer' + questionIndex);
                    c.attr('type', 'radio');
                    $(v).find('label.form-check-label').attr('for', 'correctAnswer' + questionIndex + (i + 1));
                } else {
                    $(v).remove();
                }
            });
            questionWarpper.append(questionsTemplate);
            if (questionWarpper.find('div.question-content[data-isremove="false"]').length > 1) {
                this.$surveyForm.find('button.remove-surveys-btn').removeClass('d-none');
            }
        },
        removeQuestion: function (e) {
            var ev = e || window.event,
                src = ev.srcElement || ev.target,
                questionWarpper = this.$surveyForm.find('div.question-warpper'),
                currentQuestion = $(src).closest('div.question-content'),
                currentQuestions = questionWarpper.find('div.question-content'),
                questionId = currentQuestion.attr('data-questionid');
            if (currentQuestions.length <= 1) return;
            if (questionId > 0) {
                currentQuestion.addClass("d-none").attr("data-isremove", "true");
            } else {
                currentQuestion.remove();
            }
            this.$surveyForm.find('div.question-content[data-isremove="false"]').each(function (i, v) {
                $(v).find("small.surveys-question-index").text(i + 1);
            });
            if (questionWarpper.find('div.question-content[data-isremove="false"]').length <= 1) {
                this.$surveyForm.find('button.remove-surveys-btn').addClass('d-none');
            } else {
                this.$surveyForm.find('button.remove-surveys-btn').removeClass('d-none');
            }
        },
        questionType: function (e) {
            var ev = e || window.event,
                src = ev.srcElement || ev.target,
                value = $(src).val(),
                question_content = $(src).closest('div.question-content'),
                questionOptions = question_content.find('div.question-option');
            questionOptions.each(function (i, v) {
                var c = $(v).find('div.option-correct-answer input.form-check-input');
                if (value == 1) {
                    c.attr('type', 'radio').prop("checked", false);
                } else {
                    c.attr('type', 'checkbox').prop("checked", false);
                }
            });
        },
        addOption: function (e) {
            var ev = e || window.event,
                src = ev.srcElement || ev.target,
                question_content = $(src).closest('div.question-content'),
                questionIndex = question_content.find('small.surveys-question-index').text(),
                question_option_warpper = question_content.find('div.question-option-wrapper'),
                optionTemplate = question_content.find('div.question-option').eq(0).clone();
            optionTemplate.removeAttr("data-optionid").attr("data-isremove", "false").removeClass("d-none");
            optionTemplate.find('.question-option-value').val("");
            optionTemplate.find('.form-check-input').prop('checked', false);
            question_option_warpper.append(optionTemplate);
            var questionOptions = question_option_warpper.find('div.question-option');
            if (questionOptions.length > 2) {
                questionOptions.find('button.remove-option-btn').removeClass('d-none');
            } else {
                questionOptions.find('button.remove-option-btn').addClass('d-none');
            }
            questionOptions.each(function (i, v) {
                $(v).attr('data-index', i + 1);
                var c = $(v).find('div.option-correct-answer input.form-check-input');
                c.attr('id', 'correctAnswer' + questionIndex + (i + 1));
                c.attr('name', 'correctAnswer' + questionIndex);
                $(v).find('label.form-check-label').attr('for', 'correctAnswer' + questionIndex + (i + 1));
            });
        },
        removeOption: function (e) {
            var ev = e || window.event,
                src = ev.srcElement || ev.target,
                currentQuestion_option = $(src).closest('div.question-option'),
                optionId = currentQuestion_option.attr("data-optionid"),
                question_content = $(src).closest('div.question-content'),
                questionIndex = question_content.find('small.surveys-question-index').text();
            if (optionId > 0) {
                currentQuestion_option.addClass("d-none").attr("data-isremove", "true");
            } else {
                currentQuestion_option.remove();
            }
            var questionOptions = question_content.find('div.question-option[data-isremove="false"]');
            questionOptions.each(function (i, v) {
                $(v).attr('data-index', i + 1);
                var c = $(v).find('div.option-correct-answer input.form-check-input');
                c.attr('id', 'correctAnswer' + questionIndex + (i + 1));
                c.attr('name', 'correctAnswer' + questionIndex);
                $(v).find('label.form-check-label').attr('for', 'correctAnswer' + questionIndex + (i + 1));
            });
            if (questionOptions.length > 2) {
                questionOptions.find('button.remove-option-btn').removeClass('d-none');
            } else {
                questionOptions.find('button.remove-option-btn').addClass('d-none');
            }
        },
        submitSurveys: async function () {
            var surveyForm = this.$surveyForm[0],
                url = surveyForm.action,
                button = surveyForm.querySelector('button.btn-submit-surveys'),
                formData = new FormData(surveyForm),
                questionData = this.collectQuestionData();
            formData.append('surveyQuestionArrStr', JSON.stringify(questionData));
            $qar.setBtnStatus(button, 'loading');
            try {
                const res = await fetch(url, { method: "POST", body: formData });
                const resData = await res.json();
                if (resData.status === "success") {
                    $qar.showMessage("success", resData.message, 1500);
                    if (resData.backUrl) {
                        setTimeout(() => {
                            window.location.href = resData.backUrl;
                        }, 1000);
                    }
                } else {
                    if (resData.data) {
                        const input = surveyForm.querySelector('[name="' + resData.data + '"]');
                        if (input) {
                            $qar.showFormInputError(input, resData.message);
                            return;
                        }
                    }
                    $qar.showMessage(resData.status, resData.message);
                }
            } catch (err) {
                $qar.showMessage("error", err.message);
            } finally {
                $qar.setBtnStatus(button, 'reset');
            }
        },
        collectQuestionData: function () {
            var survey_questionList = this.$surveyForm.find('div.question-content'),
                arr = [];
            for (var i = 0; i < survey_questionList.length; i++) {
                var q = $(survey_questionList[i]),
                    questionId = q.attr("data-questionid") ? parseInt(q.attr("data-questionid")) : 0,
                    isDelete = q.attr("data-isremove") === "true" ? 1 : 0,
                    questionTitle = q.find('textarea.survey-question-title').val() || "",
                    questionType = q.find('input.survey-question-type:checked').val() || "1",
                    optionList = [],
                    options = q.find('div.question-option');
                for (var j = 0; j < options.length; j++) {
                    var o = $(options[j]),
                        oId = o.attr("data-optionid") ? parseInt(o.attr("data-optionid")) : 0,
                        oDel = o.attr("data-isremove") === "true" ? 1 : 0,
                        oTitle = o.find('.question-option-value').val() || "",
                        cAnswer = o.find('.option-correct-answer input.form-check-input').is(':checked') ? 1 : 0,
                        oIndex = parseInt(o.attr('data-index')) || (j + 1);
                    optionList.push({
                        optionId: oId,
                        isDelete: oDel,
                        optionTitle: oTitle,
                        correctAnswer: cAnswer,
                        optionIndex: oIndex
                    });
                }
                arr.push({
                    questionId: questionId,
                    isDelete: isDelete,
                    questionIndex: i + 1,
                    questionTitle: questionTitle.trim(),
                    questionType: parseInt(questionType),
                    optionList: optionList
                });
            }
            return arr;
        }
    };
    $.QSurveys = new QSurveys();
})();
