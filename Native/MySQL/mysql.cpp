#include "mysql.h"

#include <cppconn/prepared_statement.h>
#include <cppconn/statement.h>

#define QUEUE_TASK(p_task)      \
{                               \
	QueueItem item;             \
	item.task = p_task;         \
	item.user_data = user_data; \
	item_queue.push(item);      \
}

#define LOCK() mutex->lock()

#define UNLOCK() mutex->unlock()

#define UNLOCK_AND_POST() \
UNLOCK();                 \
semaphore->post()

#define PRINT_SQL_ERROR(p_e) ERR_PRINT(String(e.what()) + ". MySQL error code: " + String::num_int64(e.getErrorCode()) + ". SQLState: " + e.getSQLStateCStr())

#define CALL(p_func_ref) 

using namespace godot;


void MySQL::_connect_to_database(Object *p_target, const String &p_callback, const Array &p_args) {
    bool success = false;

    LOCK();

    try {
		driver = sql::mysql::get_mysql_driver_instance();

		connection.reset(driver->connect(connection_properties));

		success = true;
    } catch(sql::SQLException &e) {
        PRINT_SQL_ERROR(e);
    }

    _call_function(p_target, p_callback, success, p_args);

    UNLOCK();
}

void MySQL::_set_schema(const String &p_schema, Object *p_target, const String &p_callback, const Array &p_args) {
	bool success = false;

	LOCK();

	try {
		if (_is_connected_to_database()) {
			connection_properties["schema"] = p_schema.utf8().get_data();
			connection->setSchema(godot_string_to_sql(p_schema));
			success = true;
		}
	} catch (sql::SQLException &e) {
		PRINT_SQL_ERROR(e);

	}
	_call_function(p_target, p_callback, success, p_args);

	UNLOCK();
}

void MySQL::_execute_query(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args) {
	bool success = false;

	LOCK();
	
	try {
		if (_is_connected_to_database()) {
			std::unique_ptr<sql::Statement> statement(connection->createStatement());
			statement->execute(godot_string_to_sql(p_query));

			success = true;
		}
	} catch (sql::SQLException &e) {
		PRINT_SQL_ERROR(e);
	}

	_call_function(p_target, p_callback, success, p_args);

    UNLOCK();
}

void MySQL::_execute_prepared_query(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args) {
	bool success = false;

	LOCK();
	
	try {
		if (_is_connected_to_database()) {
			std::unique_ptr<sql::PreparedStatement> prepared_statement(connection->prepareStatement(godot_string_to_sql(p_query)));
			_prepare_statement(prepared_statement, p_params);

			prepared_statement->execute();

			success = true;
		}
	} catch (sql::SQLException &e) {
		PRINT_SQL_ERROR(e);
	}

	_call_function(p_target, p_callback, success, p_args);

	UNLOCK();
}

void MySQL::_execute_update_query(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args) {
	bool success = false;
	int rows = 0;

	LOCK();
	
	try {
		if (_is_connected_to_database()) {
			std::unique_ptr<sql::Statement> statement(connection->createStatement());
			rows = statement->executeUpdate(godot_string_to_sql(p_query));
			success = true;
		}
	} catch (sql::SQLException &e) {
		PRINT_SQL_ERROR(e);
	}

	_call_function(p_target, p_callback, success, rows, p_args);

	UNLOCK();
}

void MySQL::_execute_prepared_update_query(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args) {
	bool success = false;
	int rows = 0;

	LOCK();
	
	try {
		if (_is_connected_to_database()) {
			std::unique_ptr<sql::PreparedStatement> prepared_statement(connection->prepareStatement(godot_string_to_sql(p_query)));
			_prepare_statement(prepared_statement, p_params);
			rows = prepared_statement->executeUpdate();
			success = true;
		}
	} catch (sql::SQLException &e) {
		PRINT_SQL_ERROR(e);
	}

	_call_function(p_target, p_callback, success, rows, p_args);

	UNLOCK();
}

void MySQL::_execute_select_query(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args) {
	bool success = false;
	size_t rows = 0;

	LOCK();

	try {
		if (_is_connected_to_database()) {
			std::unique_ptr<sql::Statement> statement(connection->createStatement());
			std::unique_ptr<sql::ResultSet> result_set(statement->executeQuery(godot_string_to_sql(p_query)));
			
			rows = result_set->rowsCount();
			success = true;
		}
	} catch (sql::SQLException &e) {
		PRINT_SQL_ERROR(e);
	}

	_call_function(p_target, p_callback, success, p_args);

	UNLOCK();
}

void MySQL::_execute_prepared_select_query(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args) {
	bool success = false;
	size_t rows = 0;

	LOCK();

	try {
		if (_is_connected_to_database()) {
			std::unique_ptr<sql::PreparedStatement> prepared_statement(connection->prepareStatement(godot_string_to_sql(p_query)));
			_prepare_statement(prepared_statement, p_params);

			std::unique_ptr<sql::ResultSet> result_set(prepared_statement->executeQuery());

			rows = result_set->rowsCount();
			success = true;
		}
	} catch (sql::SQLException &e) {
		PRINT_SQL_ERROR(e);
	}

	_call_function(p_target, p_callback, success, rows, p_args);

	UNLOCK();
}

void MySQL::_fetch_array(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args) {
	bool success = false;
	Array result_array;

	LOCK();
	
	try {
		if (_is_connected_to_database()) {
			std::unique_ptr<sql::Statement> statement(connection->createStatement());
			std::unique_ptr<sql::ResultSet> result_set(statement->executeQuery(godot_string_to_sql(p_query)));
			_process_result_set_as_array(result_set, &result_array);

			success = true;
		}
	} catch (sql::SQLException &e) {
		PRINT_SQL_ERROR(e);
	}

	_call_function(p_target, p_callback, success, result_array, p_args);

	UNLOCK();
}

void MySQL::_fetch_prepared_array(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args) {
	bool success = false;
	Array result_array;

	LOCK();
	
	try {
		if (_is_connected_to_database()) {
			std::unique_ptr<sql::PreparedStatement> prepared_statement(connection->prepareStatement(godot_string_to_sql(p_query)));
			_prepare_statement(prepared_statement, p_params);

			std::unique_ptr<sql::ResultSet> result_set(prepared_statement->executeQuery());
			_process_result_set_as_array(result_set, &result_array);

			success = true;
		}
	} catch (sql::SQLException &e) {
		PRINT_SQL_ERROR(e);
	}

	_call_function(p_target, p_callback, success, result_array, p_args);

	UNLOCK();
}

void MySQL::_fetch_dictionary(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args) {
	bool success = false;
	Array result_array;

	LOCK();
	
	try {
		if (_is_connected_to_database()) {
			std::unique_ptr<sql::Statement> statement(connection->createStatement());
			std::unique_ptr<sql::ResultSet> result_set(statement->executeQuery(godot_string_to_sql(p_query)));

			_process_result_set_as_dictionary(result_set, &result_array);

			success = true;
		}
	} catch (sql::SQLException &e) {
		PRINT_SQL_ERROR(e);
	}

	_call_function(p_target, p_callback, success, result_array, p_args);

	UNLOCK();
}

void MySQL::_fetch_prepared_dictionary(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args) {
	bool success = false;
	Array result_array;

	LOCK();
	
	try {
		if (_is_connected_to_database()) {
			std::unique_ptr<sql::PreparedStatement> prepared_statement(connection->prepareStatement(godot_string_to_sql(p_query)));
			_prepare_statement(prepared_statement, p_params);

			std::unique_ptr<sql::ResultSet> result_set(prepared_statement->executeQuery());
			_process_result_set_as_dictionary(result_set, &result_array);

			success = true;
		}
	} catch (sql::SQLException &e) {
		PRINT_SQL_ERROR(e);
	}
	
	_call_function(p_target, p_callback, success, result_array, p_args);

	UNLOCK();
}

void MySQL::_close_connection(Object *p_target, const String &p_callback, const Array &p_args) {
	_close_connection();

	LOCK();

	_call_function(p_target, p_callback, p_args);

	UNLOCK();
}

void MySQL::_close_connection() {
	LOCK();

	if (connection.get() && !connection->isClosed()) {
		connection->close();
	}

	UNLOCK();
}

bool MySQL::_is_connected_to_database() const {
	return connection.get() != nullptr && connection->isValid() && connection->reconnect();
}

bool MySQL::_is_sql_datetime(const String& p_datetime) {
	if (p_datetime.length() >= 19 && p_datetime.length() <= 26) {
		if (p_datetime[4] == '-' && p_datetime[7] == '-' && p_datetime[10] == ' ' && p_datetime[13] == ':' && p_datetime[15] == ':') {
			return true;
		}
	} else if (p_datetime.length() >= 8 && p_datetime.length() <= 16) {
		if (p_datetime[0] == '-' || (p_datetime[3] == ':' && p_datetime[5] == ':') || (p_datetime[2] == ':' && p_datetime[4] == ':')) {
			return true;
		} else if (p_datetime.length() == 10) {
			if (p_datetime[4] == '-' && p_datetime[7] == '-') {
				return true;
			}
		}
	} else if (p_datetime.length() == 4) {
		return true;
	}

	return false;
}

void MySQL::_prepare_statement(const std::unique_ptr<sql::PreparedStatement> &p_prepared_statement, const Array &p_params) {
	for (int32_t i = 0; i < p_params.size(); i++) {
		switch (p_params[i].get_type()) {
			case Variant::Type::NIL: {
				p_prepared_statement->setNull(i + 1, sql::DataType::SQLNULL);
			} break;
			case Variant::Type::BOOL: {
				p_prepared_statement->setBoolean(i + 1, p_params[i]);
			} break;
			case Variant::Type::INT: {
				p_prepared_statement->setInt64(i + 1, p_params[i]);
			} break;
			case Variant::Type::REAL: {
				p_prepared_statement->setDouble(i + 1, p_params[i]);
			} break;
			case Variant::Type::STRING: {
				if (_is_sql_datetime(p_params[i])) {
					p_prepared_statement->setDateTime(i + 1, godot_string_to_sql(p_params[i]));
				} else {
					p_prepared_statement->setString(i + 1, godot_string_to_sql(p_params[i]));
				}
			} break;
			default: {
				WARN_PRINT("Parameter " + String::num_int64(i) + " of type " + String::num_int64(p_params[i].get_type()) + " is not allowed.");
			} break;
		}
	}
}

void MySQL::_process_result_set_as_dictionary(const std::unique_ptr<sql::ResultSet> &p_result_set, Array *p_result_array) {
	sql::ResultSetMetaData *result_meta_data = p_result_set->getMetaData();

	while (p_result_set->next()) {
		Dictionary row;
		for (uint32_t i = 1; i <= result_meta_data->getColumnCount(); i++) {
			switch (result_meta_data->getColumnType(i)) {
				case sql::DataType::BIT: {
					row[sql_string_to_godot(result_meta_data->getColumnName(i))] = p_result_set->getBoolean(i);
				} break;
				case sql::DataType::TINYINT:
				case sql::DataType::SMALLINT:
				case sql::DataType::MEDIUMINT:
				case sql::DataType::BIGINT: {
					if (result_meta_data->isSigned(i)) {
						row[sql_string_to_godot(result_meta_data->getColumnName(i))] = p_result_set->getInt64(i);
					} else {
						row[sql_string_to_godot(result_meta_data->getColumnName(i))] = p_result_set->getUInt64(i);
					}
				} break;
				case sql::DataType::REAL:
				case sql::DataType::DOUBLE:
				case sql::DataType::DECIMAL:
				case sql::DataType::NUMERIC: {
					row[sql_string_to_godot(result_meta_data->getColumnName(i))] = (double)p_result_set->getDouble(i);
				} break;
				default: {
					row[sql_string_to_godot(result_meta_data->getColumnName(i))] = sql_string_to_godot(p_result_set->getString(i));
				} break;
			}
		}
		p_result_array->push_back(row);
	}
}

void MySQL::_process_result_set_as_array(const std::unique_ptr<sql::ResultSet> &p_result_set, Array *p_result_array) {
	sql::ResultSetMetaData *result_meta_data = p_result_set->getMetaData();

	while (p_result_set->next()) {
		Array row;
		for (uint32_t i = 1; i <= result_meta_data->getColumnCount(); i++) {
			switch (result_meta_data->getColumnType(i)) {
				case sql::DataType::BIT: {
					row.push_back(p_result_set->getBoolean(i));
				} break;
				case sql::DataType::TINYINT:
				case sql::DataType::SMALLINT:
				case sql::DataType::MEDIUMINT:
				case sql::DataType::BIGINT: {
					if (result_meta_data->isSigned(i)) {
						row.push_back(p_result_set->getInt(i));
					} else {
						row.push_back(p_result_set->getUInt(i));
					}
				} break;
				case sql::DataType::REAL:
				case sql::DataType::DOUBLE:
				case sql::DataType::DECIMAL:
				case sql::DataType::NUMERIC: {
					row.push_back((double)p_result_set->getDouble(i));
				} break;
				default: {
					row.push_back(sql_string_to_godot(p_result_set->getString(i)));
				} break;
			}
		}
		p_result_array->push_back(row);
	}
}

void MySQL::_thread() {
	while (!exit) {
		semaphore->wait();
		LOCK();

		if (item_queue.size()) {
			QueueItem item = item_queue.front();
			item_queue.pop();
			UNLOCK();

			switch (item.task) {
				case Task::CONNECT_TO_DATABSE: {

					if (item.user_data.size() == 3) {
						_connect_to_database(item.user_data[0], item.user_data[1], item.user_data[2]);
					}
				} break;
				case Task::SET_SCHEMA: {

					if (item.user_data.size() == 4) {
						_set_schema(item.user_data[0], item.user_data[1], item.user_data[2], item.user_data[3]);
					}
				} break;
				case Task::EXECUTE_QUERY: {

					if (item.user_data.size() == 4) {
						_execute_query(item.user_data[0], item.user_data[1], item.user_data[2], item.user_data[3]);
					}
				} break;
				case Task::EXECUTE_PREPARED_QUERY: {

					if (item.user_data.size() == 5) {
						_execute_prepared_query(item.user_data[0], item.user_data[1], item.user_data[2], item.user_data[3], item.user_data[4]);
					}
				} break;
				case Task::EXECUTE_UPDATE_QUERY: {

					if (item.user_data.size() == 4) {
						_execute_update_query(item.user_data[0], item.user_data[1], item.user_data[2], item.user_data[3]);
					}
				} break;
				case Task::EXECUTE_PREPARED_UPDATE_QUERY: {

					if (item.user_data.size() == 5) {
						_execute_prepared_update_query(item.user_data[0], item.user_data[1], item.user_data[2], item.user_data[3], item.user_data[4]);
					}
				} break;
				case Task::EXECUTE_SELECT_QUERY: {

					if (item.user_data.size() == 4) {
						_execute_select_query(item.user_data[0], item.user_data[1], item.user_data[2], item.user_data[3]);
					}
				} break;
				case Task::EXECUTE_PREPARED_SELECT_QUERY: {

					if (item.user_data.size() == 5) {
						_execute_prepared_select_query(item.user_data[0], item.user_data[1], item.user_data[2], item.user_data[3], item.user_data[4]);
					}
				} break;
				case Task::FETCH_ARRAY: {

					if (item.user_data.size() == 4) {
						_fetch_array(item.user_data[0], item.user_data[1], item.user_data[2], item.user_data[3]);
					}
				} break;
				case Task::FETCH_PREPARED_ARRAY: {

					if (item.user_data.size() == 5) {
						_fetch_prepared_array(item.user_data[0], item.user_data[1], item.user_data[2], item.user_data[3], item.user_data[4]);
					}
				} break;
				case Task::FETCH_DICTIONARY: {

					if (item.user_data.size() == 4) {
						_fetch_dictionary(item.user_data[0], item.user_data[1], item.user_data[2], item.user_data[3]);
					}
				} break;
				case Task::FETCH_PREPARED_DICTIONARY: {

					if (item.user_data.size() == 5) {
						_fetch_prepared_dictionary(item.user_data[0], item.user_data[1], item.user_data[2], item.user_data[3], item.user_data[4]);
					}
				} break;
				case Task::CLOSE_CONNECTION: {

					if (item.user_data.size() == 3) {
						_close_connection(item.user_data[0], item.user_data[1], item.user_data[2]);
					}
				} break;
				default: {
				} break;
			}
		} else {
			UNLOCK();
		}
	}
}


void MySQL::_init() { 
}

void MySQL::thread_func(const Array &p_data) {
	MySQL *mysql = Object::cast_to<MySQL>(p_data[0]);
	mysql->_thread();
}

void MySQL::set_credentials(const String &p_host, const String &p_username, const String &p_password, int p_port) {
	LOCK();

	connection_properties["hostName"] = p_host.utf8().get_data();
	connection_properties["port"] = p_port;
	connection_properties["userName"] = p_username.utf8().get_data();
	connection_properties["password"] = p_password.utf8().get_data();

	UNLOCK();
}

void MySQL::connect_to_database(Object *p_target, const String &p_callback, const Array &p_args) {
    LOCK();

    if (!thread->is_active()) {
        Array data;
        data.push_back(this);
        thread->start(this, "thread_func", data);
    }

    Array user_data;
    user_data.push_back(p_target);
    user_data.push_back(p_callback);
    user_data.push_back(p_args);
    
    QUEUE_TASK(Task::CONNECT_TO_DATABSE);

    UNLOCK_AND_POST();
}

void MySQL::execute_query(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args) {
	LOCK();

	Array user_data;
	user_data.push_back(p_query);
	user_data.push_back(p_target);
	user_data.push_back(p_callback);
	user_data.push_back(p_args);

	QUEUE_TASK(Task::EXECUTE_QUERY);

    UNLOCK_AND_POST();
}

void MySQL::execute_prepared_query(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args) {
	LOCK();

	Array user_data;
	user_data.push_back(p_query);
	user_data.push_back(p_params);
	user_data.push_back(p_target);
	user_data.push_back(p_callback);
	user_data.push_back(p_args);

	QUEUE_TASK(Task::EXECUTE_PREPARED_QUERY);

    UNLOCK_AND_POST();
}

void MySQL::execute_update_query(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args) {
	LOCK();

	Array user_data;
	user_data.push_back(p_query);
	user_data.push_back(p_target);
	user_data.push_back(p_callback);
	user_data.push_back(p_args);

	QUEUE_TASK(Task::EXECUTE_UPDATE_QUERY);

    UNLOCK_AND_POST();
}

void MySQL::execute_prepared_update_query(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args) {
	LOCK();

	Array user_data;
	user_data.push_back(p_query);
	user_data.push_back(p_params);
	user_data.push_back(p_target);
	user_data.push_back(p_callback);
	user_data.push_back(p_args);

	QUEUE_TASK(Task::EXECUTE_PREPARED_UPDATE_QUERY);

    UNLOCK_AND_POST();
}

void MySQL::execute_select_query(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args) {
	LOCK();

	Array user_data;
	user_data.push_back(p_query);
	user_data.push_back(p_target);
	user_data.push_back(p_callback);
	user_data.push_back(p_args);

	QUEUE_TASK(Task::EXECUTE_SELECT_QUERY);

    UNLOCK_AND_POST();
}

void MySQL::execute_prepared_select_query(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args) {
	LOCK();

	Array user_data;
	user_data.push_back(p_query);
	user_data.push_back(p_params);
	user_data.push_back(p_target);
	user_data.push_back(p_callback);
	user_data.push_back(p_args);

	QUEUE_TASK(Task::EXECUTE_PREPARED_SELECT_QUERY);

    UNLOCK_AND_POST();
}

void MySQL::fetch_array(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args) {
	LOCK();

	Array user_data;
	user_data.push_back(p_query);
	user_data.push_back(p_target);
	user_data.push_back(p_callback);
	user_data.push_back(p_args);

	QUEUE_TASK(Task::FETCH_ARRAY);

    UNLOCK_AND_POST();
}

void MySQL::fetch_prepared_array(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args) {
	LOCK();

	Array user_data;
	user_data.push_back(p_query);
	user_data.push_back(p_params);
	user_data.push_back(p_target);
	user_data.push_back(p_callback);
	user_data.push_back(p_args);

	QUEUE_TASK(Task::FETCH_PREPARED_ARRAY);

    UNLOCK_AND_POST();
}

void MySQL::fetch_dictionary(const String &p_query, Object *p_target, const String &p_callback, const Array &p_args) {
	LOCK();

	Array user_data;
	user_data.push_back(p_query);
	user_data.push_back(p_target);
	user_data.push_back(p_callback);
	user_data.push_back(p_args);

	QUEUE_TASK(Task::FETCH_DICTIONARY);

    UNLOCK_AND_POST();
}

void MySQL::fetch_prepared_dictionary(const String &p_query, const Array &p_params, Object *p_target, const String &p_callback, const Array &p_args) {
	LOCK();

	Array user_data;
	user_data.push_back(p_query);
	user_data.push_back(p_params);
	user_data.push_back(p_target);
	user_data.push_back(p_callback);
	user_data.push_back(p_args);

	QUEUE_TASK(Task::FETCH_PREPARED_DICTIONARY);

    UNLOCK_AND_POST();
}

void MySQL::set_schema(const String &p_schema, Object *p_target, const String &p_callback, const Array &p_args) {
	LOCK();

	Array user_data;
	user_data.push_back(p_schema);
	user_data.push_back(p_target);
	user_data.push_back(p_callback);
	user_data.push_back(p_args);

	QUEUE_TASK(Task::SET_SCHEMA);

    UNLOCK_AND_POST();
}

void MySQL::close_connection(Object *p_target, const String &p_callback, const Array &p_args) {
	LOCK();

	Array user_data;
	user_data.push_back(p_target);
	user_data.push_back(p_callback);
	user_data.push_back(p_args);

	QUEUE_TASK(Task::CLOSE_CONNECTION);

	UNLOCK_AND_POST();
}

void MySQL::_register_methods() {
    register_method("set_credentials", &MySQL::set_credentials);

    register_method("connect_to_database", &MySQL::connect_to_database);
    register_method("set_schema", &MySQL::set_schema);

    register_method("execute_query", &MySQL::execute_query);
    register_method("execute_prepared_query", &MySQL::execute_prepared_query);

    register_method("execute_update_query", &MySQL::execute_update_query);
    register_method("execute_prepared_update_query", &MySQL::execute_prepared_update_query);

    register_method("execute_select_query", &MySQL::execute_select_query);
    register_method("execute_prepared_select_query", &MySQL::execute_prepared_select_query);

    register_method("fetch_array", &MySQL::fetch_array);
    register_method("fetch_prepared_array", &MySQL::fetch_prepared_array);

    register_method("fetch_dictionary", &MySQL::fetch_dictionary);
    register_method("fetch_prepared_dictionary", &MySQL::fetch_prepared_dictionary);

    register_method("close_connection", &MySQL::close_connection);
    register_method("thread_func", &MySQL::thread_func); //? ???
}

MySQL::MySQL() {
    connection_properties["OPT_RECONNECT"] = true;

    driver = nullptr;
    exit = false;
}

MySQL::~MySQL() {
    exit = true;
    semaphore->post();
    thread->wait_to_finish();

    _close_connection();
}

#undef PRINT_SQL_ERROR
#undef UNLOCK_AND_POST
#undef UNLOCK
#undef LOCK
#undef QUEUE_TASK